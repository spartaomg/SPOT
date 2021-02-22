Imports System.IO

Friend Module ModOptimize
    Public CmdArg As String()
    Public CmdLn As Boolean = False

    Public KLA() As Byte
    Public OrigBitmap, C64Bitmap, DiagBitmap As Bitmap
    Public SHBitmap, SLBitmap, CRBitmap, ACBitmap As Bitmap
    Public InputFilePath As String
    Public PicW As Integer
    Public PicH As Integer
    Public CharCol As Integer
    Public CharRow As Integer
    Public ReadOnly RtoC64ConvTab() As Byte = My.Resources.PaletteConvTab
    Public ReadOnly C64Palettes() As Byte = My.Resources.C64Palettes
    Public PaletteCnt As Integer = C64Palettes.Count / 48
    Public Pic(), PicMsk() As Byte   'Original picture array
    Public BMP() As Byte   'C64 bitmap array
    Public ColTab0(), ColTab1(), ColTab2(), ColTab3() As Byte  'For color tabs
    Public BGCol, BGCols(-1) As Byte   'Background color

    Public MUC(15) As Byte

    Public ColRAM() As Byte
    Public ScrHi(), ScrLo(), ScrRAM() As Byte
    Public Blank0(), Blank1(), Blank2() As Byte

    Public ReadOnly UnusedColor As Byte = &H10  'Value not used by either the image or the C64
    Public FPath, FName, FExt, SpotFolder, SavePath, SaveName, SaveExt, CmdIn, CmdOptions, CmdOut, CmdColors As String
    Public C64Formats As Boolean = False
    Public FromKla As Boolean = False
    Public ScrollPos As Point

    Public Function OptimizeKla() As Boolean
        On Error GoTo Err

        If CmdLn = False Then
            FrmSPOT.TSSL.Text = "Opening Koala file: " + InputFilePath
            FrmSPOT.Refresh()
        End If

        PicW = 160
        PicH = 200
        CharCol = Int(PicW / 4)
        CharRow = Int(PicH / 8)

        ReDim Pic((PicW * PicH) - 1)
        ReDim ColTab0((CharCol * CharRow) - 1), ColTab1((CharCol * CharRow) - 1), ColTab2((CharCol * CharRow) - 1), ColTab3((CharCol * CharRow) - 1)
        ReDim BMP((CharCol * PicH) - 1)

        For I As Integer = 0 To 999
            ColTab1(I) = Int(KLA(8002 + I) / 16)
            ColTab2(I) = KLA(8002 + I) Mod 16
            ColTab3(I) = KLA(9002 + I) And &HF
        Next

        ReDim BGCols(0)
        BGCols(0) = KLA(10002)
        BGCol = KLA(10002)

        C64Bitmap = New Bitmap(PicW * 2, PicH, Imaging.PixelFormat.Format32bppArgb)
        OrigBitmap = New Bitmap(PicW * 2, PicH, Imaging.PixelFormat.Format32bppArgb)
        DiagBitmap = New Bitmap(PicW * 2, PicH, Imaging.PixelFormat.Format32bppArgb)

        Dim CI As Integer       'Color Tab index
        Dim PI As Integer       'Pixel Index
        Dim BitMask As Byte
        Dim Bits As Byte
        Dim Col As Byte

        For Y As Integer = 0 To PicH - 1
            For X As Integer = 0 To PicW - 1
                CI = (Int(Y / 8) * CharCol) + Int(X / 4)    'ColorTab index from X and Y
                PI = (8 * Int(X / 4)) + (Y Mod 8) + (Int(Y / 8) * PicW * 2) 'Pixel index in Bitmap
                Select Case X Mod 4
                    Case 0
                        BitMask = &HC0
                    Case 1
                        BitMask = &H30
                    Case 2
                        BitMask = &HC
                    Case 3
                        BitMask = &H3
                End Select
                Bits = KLA(2 + PI) And BitMask
                Select Case Bits
                    Case 0
                        Col = BGCol
                    Case &H1, &H4, &H10, &H40
                        Col = ColTab1(CI)
                    Case &H2, &H8, &H20, &H80
                        Col = ColTab2(CI)
                    Case &H3, &HC, &H30, &HC0
                        Col = ColTab3(CI)
                End Select

                Dim RGB As Color = Color.FromArgb(C64Palettes(Col), C64Palettes(Col + 16), C64Palettes(Col + 32))

                C64Bitmap.SetPixel(X * 2, Y, RGB)
                C64Bitmap.SetPixel((X * 2) + 1, Y, RGB)
                OrigBitmap.SetPixel(X * 2, Y, RGB)
                OrigBitmap.SetPixel((X * 2) + 1, Y, RGB)
            Next
        Next

        FrmSPOT.PbxPic.Image = C64Bitmap

        OptimizeKla = OptimizePng()

        Exit Function

Err:
        MsgBox(ErrorToString(), vbOKOnly + vbExclamation, Reflection.MethodBase.GetCurrentMethod.Name + " Error")
        OptimizeKla = False

        If CmdLn = False Then
            FrmSPOT.TSSL.Text = "Image could not be converted :("
            FrmSPOT.Refresh()
        End If

    End Function

    Public Function ConvertPicToC64Palette() As Boolean
        On Error GoTo Err

        ConvertPicToC64Palette = True

        If CmdLn = False Then
            FrmSPOT.TSSL.Text = "Converting palette..."
            FrmSPOT.Refresh()
        End If

        PicW = Int(OrigBitmap.Width / 2)
        PicH = OrigBitmap.Height
        CharCol = Int(PicW / 4)
        CharRow = Int(PicH / 8)

        If My.Settings.OutputKla = True Then
            If (CharCol < 40) Or (CharRow < 25) Then
                MsgBox("This image cannot be saved as Koala or Optimized Bitmap as it is smaller than 320x200 pixels!" + vbNewLine + vbNewLine +
                       "All other selected output formats will be created.", vbOKOnly + vbInformation, "Image is too small for KLA and OBM formats")
            End If
        End If

        Dim R, G, B As Byte
        Dim PicCol As Color
        Dim dR, dG, dB, uR As Integer
        Dim dH, dS, dV, BestHSV As Double
        Dim BestMatch, BestMatchIndex As Integer
        Dim BestPalette(PaletteCnt - 1) As Integer
        Dim BestPaletteIndex As Integer

        C64Bitmap = New Bitmap(OrigBitmap.Width, OrigBitmap.Height, Imaging.PixelFormat.Format32bppArgb)

        'First check if there is a direct palette match
        Dim PaletteMatch As Boolean
        For P As Integer = 0 To PaletteCnt - 1
            For Y As Integer = 0 To PicH - 1
                For X As Integer = 0 To PicW - 1
                    PaletteMatch = False
                    PicCol = OrigBitmap.GetPixel(X * 2, Y)
                    For J As Integer = 0 To 15
                        If (C64Palettes((48 * P) + J) = PicCol.R) And (C64Palettes((48 * P) + 16 + J) = PicCol.G) And (C64Palettes((48 * P) + 32 + J) = PicCol.B) Then
                            'Use default palette 
                            Dim NewColor As Color = Color.FromArgb(C64Palettes(J), C64Palettes(J + 16), C64Palettes(J + 32))

                            C64Bitmap.SetPixel(X * 2, Y, NewColor)
                            C64Bitmap.SetPixel((X * 2) + 1, Y, NewColor)
                            PaletteMatch = True
                            Exit For
                        End If
                    Next
                    If PaletteMatch = False Then Exit For
                Next
                If PaletteMatch = False Then Exit For
            Next
            If PaletteMatch = True Then Exit Function   'Direct match found, we are done
        Next

        If CmdLn = False Then
            FrmSPOT.TSSL.Text = "Direct palette match not found. Identifying best fitting C64 palette..."
            FrmSPOT.Refresh()
        End If

        'No direct match, identify the best matching C64 palette
        For Y As Integer = 0 To PicH - 1
            For X As Integer = 0 To PicW - 1
                R = OrigBitmap.GetPixel(X * 2, Y).R
                G = OrigBitmap.GetPixel(X * 2, Y).G
                B = OrigBitmap.GetPixel(X * 2, Y).B

                BestMatch = &H10000000

                For P As Integer = 0 To PaletteCnt - 1
                    For J As Integer = 0 To 15
                        uR = (R + CInt(C64Palettes((P * 48) + J))) / 2
                        dR = R - CInt(C64Palettes((P * 48) + J))
                        dG = G - CInt(C64Palettes((P * 48) + J + 16))
                        dB = B - CInt(C64Palettes((P * 48) + J + 32))
                        Dim cDiff = (((512 + uR) * dR * dR) >> 8) + (4 * dG * dG) + (((767 - uR) * dB * dB) >> 8)
                        If cDiff < BestMatch Then
                            BestMatch = cDiff
                            BestMatchIndex = J
                            BestPaletteIndex = P
                        End If
                    Next
                Next
                BestPalette(BestPaletteIndex) += 1
            Next
        Next

        BestPaletteIndex = -1
        Dim BP As Integer = 0
        For I As Integer = 0 To PaletteCnt - 1
            If BestPalette(I) > BP Then
                BP = BestPalette(I)
                BestPaletteIndex = I
            End If
        Next

        'Now use the best matching palette to match colors
        For Y As Integer = 0 To PicH - 1
            For X As Integer = 0 To PicW - 1
                R = OrigBitmap.GetPixel(X * 2, Y).R
                G = OrigBitmap.GetPixel(X * 2, Y).G
                B = OrigBitmap.GetPixel(X * 2, Y).B

                BestMatch = &H10000000

                For J As Integer = 0 To 15
                    uR = (R + CInt(C64Palettes((BestPaletteIndex * 48) + J))) / 2
                    dR = R - CInt(C64Palettes((BestPaletteIndex * 48) + J))
                    dG = G - CInt(C64Palettes((BestPaletteIndex * 48) + J + 16))
                    dB = B - CInt(C64Palettes((BestPaletteIndex * 48) + J + 32))
                    Dim cDiff = (((512 + uR) * dR * dR) >> 8) + (4 * dG * dG) + (((767 - uR) * dB * dB) >> 8)
                    If cDiff < BestMatch Then
                        BestMatch = cDiff
                        BestMatchIndex = J
                    End If
                Next

                'Use default palette 
                Dim NewColor As Color = Color.FromArgb(C64Palettes(BestMatchIndex), C64Palettes(BestMatchIndex + 16), C64Palettes(BestMatchIndex + 32))

                C64Bitmap.SetPixel(X * 2, Y, NewColor)
                C64Bitmap.SetPixel((X * 2) + 1, Y, NewColor)
            Next
        Next

        Exit Function
Err:
        MsgBox(ErrorToString(), vbOKOnly + vbExclamation, Reflection.MethodBase.GetCurrentMethod.Name + " Error")
        ConvertPicToC64Palette = False

        If CmdLn = False Then
            FrmSPOT.TSSL.Text = "Palette could not be converted :("
            FrmSPOT.Refresh()
        End If

    End Function

    Public Function OptimizePng() As Boolean
        On Error GoTo Err

        OptimizePng = True
        Dim Path As String

        If CmdLn = False Then
            'Tool window, autosave disabled -> open Save Dialog 
            If My.Settings.Autosave = False Then

                Dim SaveDLG As New SaveFileDialog
                With SaveDLG
                    .Title = "Select an output folder and a base filename to save converted files (extension will be ignored)"
                    .FileName = FName
                    .OverwritePrompt = False

                    Dim R As DialogResult = SaveDLG.ShowDialog()

                    Path = If(R = DialogResult.OK, .FileName, "")
                    If Path <> "" Then
                        Dim FInfo As FileInfo = My.Computer.FileSystem.GetFileInfo(Path)
                        SavePath = FInfo.DirectoryName
                        SaveExt = FInfo.Extension
                        SaveName = FInfo.Name 'Strings.Replace(FInfo.Name, SaveExt, "")
                        If SaveExt <> "" Then
                            SaveName = Left(SaveName, Len(SaveName) - Len(SaveExt))
                        End If
                    Else
                        MsgBox("No output folder selected", vbOKOnly, "Image not converted")
                        OptimizePng = False
                        Exit Function
                    End If
                End With

                FrmSPOT.Cursor = Cursors.WaitCursor

            Else    'Tool window, autosave enabled -> Save to SPOT folder

                SavePath = SpotFolder + "\" + FName
                SaveName = FName

            End If
        Else
            'Commandline - this part is done in FrmSPOT_Load
        End If

        'RearrangeBitmap()

        FrmSPOT.PbxPic.Image = C64Bitmap

        SHBitmap = New Bitmap(PicW * 2, PicH, Imaging.PixelFormat.Format32bppArgb)
        SLBitmap = New Bitmap(PicW * 2, PicH, Imaging.PixelFormat.Format32bppArgb)
        CRBitmap = New Bitmap(PicW * 2, PicH, Imaging.PixelFormat.Format32bppArgb)
        ACBitmap = New Bitmap(PicW * 2, PicH, Imaging.PixelFormat.Format32bppArgb)

        FrmSPOT.PbxSH.Image = SHBitmap
        FrmSPOT.PbxSL.Image = SLBitmap
        FrmSPOT.PbxCR.Image = CRBitmap
        FrmSPOT.PbxAC.Image = ACBitmap

        SaveImgFormat()

        With My.Settings
            If (.OutputKla = False) And (.OutputMap = False) And (.OutputCol = False) And (.OutputScr = False) And (.OutputCcr = False) And (.OutputObm = False) Then
                C64Formats = FromKla    'False
                'GoTo SkipOptimize
            Else
                C64Formats = True
                If (OrigBitmap.Width Mod 8 <> 0) Or (OrigBitmap.Height Mod 8 <> 0) Then
                    MsgBox("For C64 formats, the dimensions of the image must be multiples of 8", vbOKOnly + vbInformation, "Unable to convert image to C64 formats")
                    OptimizePng = False
                    Exit Function
                End If
            End If
        End With

        ReDim Pic((PicW * PicH) - 1), PicMsk((PicW * PicH) - 1)
        Dim CT0((CharCol * CharRow) - 1), CT1((CharCol * CharRow) - 1), CT2((CharCol * CharRow) - 1), CT3((CharCol * CharRow) - 1) As Byte
        ReDim ColTab0((CharCol * CharRow) - 1), ColTab1((CharCol * CharRow) - 1), ColTab2((CharCol * CharRow) - 1), ColTab3((CharCol * CharRow) - 1)
        ReDim BMP((CharCol * PicH) - 1)

        Dim V As Byte       'One byte to work with...
        Dim CP As Integer   'Char Position within array

        If CmdLn = False Then
            FrmSPOT.TSSL.Text = "Sorting colors..."
            FrmSPOT.Refresh()
        End If

        'Fetch R component of the RGB color code for each pixel, convert it to C64 palette, and save it to Pic array
        For Y As Integer = 0 To PicH - 1
            For X As Integer = 0 To PicW - 1
                Pic((Y * PicW) + X) = RtoC64ConvTab(C64Bitmap.GetPixel(X * 2, Y).R)
            Next
        Next

        'Fill all three Color Tabs with a value that is not used by either the image or the C64
        For I As Integer = 0 To CT0.Count - 1
            CT0(I) = UnusedColor
            CT1(I) = UnusedColor
            CT2(I) = UnusedColor
            CT3(I) = UnusedColor
        Next

        'Sort R values into 3 Color Tabs per char
        For CY As Integer = 0 To CharRow - 1                '200 char rows
            For CX As Integer = 0 To CharCol - 1            '40 chars per row
                CP = (CY * 8 * PicW) + (CX * 4)             'Char's position within R array (160*1600) = Y*160*8 + X*4
                For BY As Integer = 0 To 7                  'Pixel's Y-position within char
                    For BX As Integer = 0 To 3              'Pixel's X-position within char
                        V = Pic(CP + (BY * PicW) + BX)      'Fetch R value of pixel in char

                        If (CT0((CY * CharCol) + CX) = V) Or (CT1((CY * CharCol) + CX) = V) Or (CT2((CY * CharCol) + CX) = V) Or (CT3((CY * CharCol) + CX) = V) Then
                            'Color cannot be &0c, and can only be stored once
                        ElseIf CT1((CY * CharCol) + CX) = UnusedColor Then
                            'If color is not in Color Tabs yet, first fill Color Tab 1
                            CT1((CY * CharCol) + CX) = V
                        ElseIf CT2((CY * CharCol) + CX) = UnusedColor Then
                            'Then try Color Tab 2
                            CT2((CY * CharCol) + CX) = V
                        ElseIf CT3((CY * CharCol) + CX) = UnusedColor Then
                            'Finally Color Tab 3
                            CT3((CY * CharCol) + CX) = V
                        ElseIf CT0((CY * CharCol) + CX) = UnusedColor Then
                            'If color is not in Color Tabs yet, first fill Color Tab 1
                            CT0((CY * CharCol) + CX) = V
                        Else
                            MsgBox("This picture cannot be converted as it contains more than 4 colors per char block!")
                            OptimizePng = False
                            Exit Function
                        End If
                    Next
                Next
            Next
        Next

        ReDim BGCols(-1)

        Dim ColCnt As Byte
        For C As Integer = 0 To 15
            Dim ColOK As Boolean = True
            For I As Integer = 0 To CT0.Count - 1
                ColCnt = 0
                If (CT0(I) <> C) And (CT0(I) <> UnusedColor) Then ColCnt += 1
                If (CT1(I) <> C) And (CT1(I) <> UnusedColor) Then ColCnt += 1
                If (CT2(I) <> C) And (CT2(I) <> UnusedColor) Then ColCnt += 1
                If (CT3(I) <> C) And (CT3(I) <> UnusedColor) Then ColCnt += 1
                If ColCnt = 4 Then
                    ColOK = False
                    Exit For
                End If
            Next
            If ColOK = True Then
                ReDim Preserve BGCols(BGCols.Count)
                BGCols(BGCols.Count - 1) = C
            End If
        Next

        If BGCols.Count = 0 Then
            MsgBox("This picture cannot be converted as no color can be used as a background color!")
            OptimizePng = False
            Exit Function
        End If

        Dim ImgOptimized As Boolean = False

        'Optimize bitmap with all possible background color
        For C As Integer = 0 To BGCols.Count - 1
            BGCol = BGCols(C)
            Dim sCol = LCase(Right(Hex(BGCol), 1))
            'If tool is run from command line, check if the current background color is on the list
            If (CmdLn = False) Or (InStr(CmdColors, sCol) <> 0) Then
                For I As Integer = 0 To CT0.Count - 1
                    ColTab0(I) = CT0(I)
                    ColTab1(I) = CT1(I)
                    ColTab2(I) = CT2(I)
                    ColTab3(I) = CT3(I)
                    If ColTab1(I) = BGCol Then
                        ColTab1(I) = ColTab0(I)
                        ColTab0(I) = UnusedColor
                    ElseIf ColTab2(I) = BGCol Then
                        ColTab2(I) = ColTab0(I)
                        ColTab0(I) = UnusedColor
                    ElseIf ColTab3(I) = BGCol Then
                        ColTab3(I) = ColTab0(I)
                        ColTab0(I) = UnusedColor
                    End If
                Next

                If CmdLn = False Then
                    FrmSPOT.TSSL.Text = "Optimizing using background color 0" + BGCol.ToString
                    FrmSPOT.Refresh()
                End If

                Optimize()
                ImgOptimized = True
            End If
        Next

        If ImgOptimized Then
            If CmdLn = False Then
                FrmSPOT.TSSL.Text = "Updating color spaces..."
                FrmSPOT.Refresh()
            End If

            Dim ColorBG As Color = Color.FromArgb(C64Palettes(BGCol), C64Palettes(BGCol + 16), C64Palettes(BGCol + 32))

            For X As Integer = 0 To (PicW * 2) - 1
                For Y As Integer = 0 To PicH - 1
                    ACBitmap.SetPixel(X, Y, ColorBG)
                Next
            Next

            For CY As Integer = 0 To CharRow - 1
                For CX = 0 To CharCol - 1
                    CP = (CY * CharCol) + CX

                    Dim ColorSH As Color = Color.FromArgb(C64Palettes(ScrHi(CP)), C64Palettes(ScrHi(CP) + 16), C64Palettes(ScrHi(CP) + 32))
                    Dim ColorSL As Color = Color.FromArgb(C64Palettes(ScrLo(CP)), C64Palettes(ScrLo(CP) + 16), C64Palettes(ScrLo(CP) + 32))
                    Dim ColorCR As Color = Color.FromArgb(C64Palettes(ColRAM(CP)), C64Palettes(ColRAM(CP) + 16), C64Palettes(ColRAM(CP) + 32))
                    Dim ColorBl As Color = Color.FromArgb(0, 255, 0)

                    For X As Integer = 0 To 5
                        For Y As Integer = 0 To 5
                            If Blank0((CY * CharCol) + CX) = 0 Then
                                SHBitmap.SetPixel((CX * 8) + X + 1, (CY * 8) + Y + 1, ColorSH)
                            Else
                                SHBitmap.SetPixel((CX * 8) + X + 1, (CY * 8) + Y + 1, ColorBl)
                            End If
                            If Blank1((CY * CharCol) + CX) = 0 Then
                                SLBitmap.SetPixel((CX * 8) + X + 1, (CY * 8) + Y + 1, ColorSL)
                            Else
                                SLBitmap.SetPixel((CX * 8) + X + 1, (CY * 8) + Y + 1, ColorBl)
                            End If
                            If Blank2((CY * CharCol) + CX) = 0 Then
                                CRBitmap.SetPixel((CX * 8) + X + 1, (CY * 8) + Y + 1, ColorCR)
                            Else
                                CRBitmap.SetPixel((CX * 8) + X + 1, (CY * 8) + Y + 1, ColorBl)
                            End If
                        Next
                    Next

                    For X As Integer = 1 To 3
                        For Y As Integer = 1 To 3
                            If Blank0((CY * CharCol) + CX) = 0 Then
                                ACBitmap.SetPixel((CX * 8) + X, (CY * 8) + Y + 4, ColorSH)
                            Else
                                ACBitmap.SetPixel((CX * 8) + X, (CY * 8) + Y + 4, ColorBl)
                            End If
                            If Blank1((CY * CharCol) + CX) = 0 Then
                                ACBitmap.SetPixel((CX * 8) + X + 4, (CY * 8) + Y + 4, ColorSL)
                            Else
                                ACBitmap.SetPixel((CX * 8) + X + 4, (CY * 8) + Y + 4, ColorBl)
                            End If
                            If Blank2((CY * CharCol) + CX) = 0 Then
                                ACBitmap.SetPixel((CX * 8) + X + 4, (CY * 8) + Y, ColorCR)
                            Else
                                ACBitmap.SetPixel((CX * 8) + X + 4, (CY * 8) + Y, ColorBl)
                            End If
                        Next
                    Next

                Next
            Next
        Else
            MsgBox("The selected background color(s) could not be used!", vbOKOnly + vbInformation, "Invalid background color(s)")
        End If

SkipOptimize:

        FrmSPOT.PbxPic.BringToFront()
        ResizePbx()

        Exit Function

Err:
        MsgBox(ErrorToString(), vbOKOnly + vbExclamation, Reflection.MethodBase.GetCurrentMethod.Name + " Error")
        OptimizePng = False

        If CmdLn = False Then
            FrmSPOT.TSSL.Text = "Image could not be converted :("
            FrmSPOT.Refresh()
        End If

    End Function

    Private Sub Optimize()
        On Error GoTo Err

        Dim C64Col(17) As Integer
        ReDim ColRAM(ColTab1.Count - 1), ScrHi(ColTab1.Count - 1), ScrLo(ColTab1.Count - 1), ScrRAM(ColTab0.Count - 1)
        Dim ColMap(ColTab0.Count - 1, 16) As Byte, ColTmp(ColTab1.Count - 1) As Byte
        ReDim Blank0(ColTab1.Count - 1), Blank1(ColTab1.Count - 1), Blank2(ColTab1.Count - 1)

        For I As Integer = 0 To ColTab0.Count - 1

            ColRAM(I) = 255                     'Reset color spaces
            ScrHi(I) = 255
            ScrLo(I) = 255

            C64Col(ColTab1(I)) += 1             'Calculate frequency of colors
            C64Col(ColTab2(I)) += 1
            C64Col(ColTab3(I)) += 1
            ColMap(I, ColTab1(I)) = 255         'Create a separate color map for each color
            ColMap(I, ColTab2(I)) = 255
            ColMap(I, ColTab3(I)) = 255
        Next

        For I As Integer = 0 To 15              'Initialize most used color ranklist
            MUC(I) = I
        Next

        Dim Chg As Boolean

        If CmdLn = False Then
            FrmSPOT.TSSL.Text = "Sorting colors by frequency..."
            FrmSPOT.Refresh()
        End If

Resort:
        Chg = False
        For I = 0 To 14
            If C64Col(I) < C64Col(I + 1) Then   'Sort colors based on frequency
                Dim Tmp As Integer = C64Col(I)
                C64Col(I) = C64Col(I + 1)
                C64Col(I + 1) = Tmp
                Tmp = MUC(I)
                MUC(I) = MUC(I + 1)
                MUC(I + 1) = Tmp
                Chg = True
            End If
        Next
        If Chg = True Then GoTo Resort

        For I As Integer = 0 To ColTab1.Count - 1

            'Most used color goes to ScrHi
            If (ColTab1(I) = MUC(0)) Or (ColTab2(I) = MUC(0)) Or (ColTab3(I) = MUC(0)) Then ScrHi(I) = MUC(0)
            'Second most used color goes to ScrLo
            If (ColTab1(I) = MUC(1)) Or (ColTab2(I) = MUC(1)) Or (ColTab3(I) = MUC(1)) Then ScrLo(I) = MUC(1)
            'Third most used color goes to ColRAM
            If (ColTab1(I) = MUC(2)) Or (ColTab2(I) = MUC(2)) Or (ColTab3(I) = MUC(2)) Then ColRAM(I) = MUC(2)

        Next

        If CmdLn = False Then
            FrmSPOT.TSSL.Text = "Remapping colors..."
            FrmSPOT.Refresh()
        End If

        Dim OverlapSH, OverlapSL, OverlapCR As Integer
        For J As Integer = 3 To 14
            OverlapSH = 0    'Overlap with ScrHi
            OverlapSL = 0    'Overlap with ScrLo
            OverlapCR = 0    'Overlap with ColRAM

            'Calculate overlap with the color spaces separately for each color
            For I = 0 To ColTab1.Count - 1
                If (ColMap(I, MUC(J)) = 255) Then
                    If (ScrHi(I) <> 255) Then
                        OverlapSH += 1
                    ElseIf (ScrLo(I) <> 255) Then
                        OverlapSL += 1
                    ElseIf (ColRAM(I) <> 255) Then
                        OverlapCR += 1
                    End If
                End If
            Next

            For I As Integer = 0 To ScrHi.Count - 1                                         'SH<SL<CR
                If (ColMap(I, MUC(J)) = 255) Then
                    If (OverlapSH <= OverlapSL) And (OverlapSH <= OverlapCR) Then           'OverlapSH is smallest
                        If OverlapSL <= OverlapCR Then                                      'OverlapSL is second smallest
                            If ScrHi(I) = 255 Then
                                ScrHi(I) = MUC(J)
                            ElseIf ScrLo(I) = 255 Then
                                ScrLo(I) = MUC(J)
                            Else
                                ColRAM(I) = MUC(J)
                            End If
                        Else                                                                'OverlapCR Is second smallest
                            If ScrHi(I) = 255 Then
                                ScrHi(I) = MUC(J)
                            ElseIf ColRAM(I) = 255 Then
                                ColRAM(I) = MUC(J)
                            Else
                                ScrLo(I) = MUC(J)
                            End If
                        End If
                    ElseIf (OverlapSL <= OverlapSH) And (OverlapSL <= OverlapCR) Then       'OverlapSL is smallest
                        If OverlapSH <= OverlapCR Then                                      'OverlapSH is second smallest
                            If ScrLo(I) = 255 Then
                                ScrLo(I) = MUC(J)
                            ElseIf ScrHi(I) = 255 Then
                                ScrHi(I) = MUC(J)
                            Else
                                ColRAM(I) = MUC(J)
                            End If
                        Else                                                                'OverlapCR is second smallest
                            If ScrLo(I) = 255 Then
                                ScrLo(I) = MUC(J)
                            ElseIf ColRAM(I) = 255 Then
                                ColRAM(I) = MUC(J)
                            Else
                                ScrHi(I) = MUC(J)
                            End If
                        End If
                    Else                                                                    'OverlapCR is smallest
                        If OverlapSH <= OverlapSL Then                                      'OverlapSH is second smallest
                            If ColRAM(I) = 255 Then
                                ColRAM(I) = MUC(J)
                            ElseIf ScrHi(I) = 255 Then
                                ScrHi(I) = MUC(J)
                            Else
                                ScrLo(I) = MUC(J)
                            End If
                        Else                                                                'OverlapSL is second smallest
                            If ColRAM(I) = 255 Then
                                ColRAM(I) = MUC(J)
                            ElseIf ScrLo(I) = 255 Then
                                ScrLo(I) = MUC(J)
                            Else
                                ScrHi(I) = MUC(J)
                            End If
                        End If
                    End If
                End If
            Next
            'MsgBox(MUC(J).ToString + vbNewLine + vbNewLine + OverlapSH.ToString + vbNewLine + OverlapSL.ToString + vbNewLine + OverlapCR.ToString)
        Next

        'File.WriteAllBytes(SpotFolder + "\ColRAM_FF.bin", ScrLo)       'ColRAM and ScrLO will be swapped later!!!
        'File.WriteAllBytes(SpotFolder + "\ScrHi_FF.bin", ScrHi)
        'File.WriteAllBytes(SpotFolder + "\ScrLo_FF.bin", ColRAM)

        '----------------------------------------------------------------------------

        If CmdLn = False Then
            FrmSPOT.TSSL.Text = "Swapping colors where possible..."
            FrmSPOT.Refresh()
        End If

        For I As Integer = 1 To ColRAM.Count - 2
            If (ScrHi(I) <> ScrHi(I - 1)) And (ScrHi(I) <> ScrHi(I + 1)) And (ScrHi(I) <> 255) And (ScrHi(I - 1) <> 255) And (ScrHi(I + 1) <> 255) Then
                If ScrLo(I) = 255 Then
                    ScrLo(I) = ScrHi(I)
                    ScrHi(I) = 255
                ElseIf ColRAM(I) = 255 Then
                    ColRAM(I) = ScrHi(I)
                    ScrHi(I) = 255
                End If
            End If
            If (ScrLo(I) <> ScrLo(I - 1)) And (ScrLo(I) <> ScrLo(I + 1)) And (ScrLo(I) <> 255) And (ScrLo(I - 1) <> 255) And (ScrLo(I + 1) <> 255) Then
                If ColRAM(I) = 255 Then
                    ColRAM(I) = ScrLo(I)
                    ScrLo(I) = 255
                ElseIf ScrHi(I) = 255 Then
                    ScrHi(I) = ScrLo(I)
                    ScrLo(I) = 255
                End If
            End If
            If (ColRAM(I) <> ColRAM(I - 1)) And (ColRAM(I) <> ColRAM(I + 1)) And (ColRAM(I) <> 255) And (ColRAM(I - 1) <> 255) And (ColRAM(I + 1) <> 255) Then
                If ScrHi(I) = 255 Then
                    ScrHi(I) = ColRAM(I)
                    ColRAM(I) = 255
                ElseIf ScrLo(I) = 255 Then
                    ScrLo(I) = ColRAM(I)
                    ColRAM(I) = 255
                End If
            End If
        Next

        '----------------------------------------------------------------------------
        'Find loner bytes that can be swapped
        For I As Integer = 1 To ColRAM.Count - 2
            'If I = 219 Then MsgBox(I.ToString)
            If (ScrHi(I) <> ScrHi(I - 1)) And (ScrHi(I) <> ScrHi(I + 1)) And (ScrHi(I) <> 255) Then
                If ScrHi(I) = ScrLo(I + 1) Then
                    If (ScrLo(I) <> ScrLo(I - 1)) Then
                        ScrHi(I) = ScrLo(I)
                        ScrLo(I) = ScrLo(I + 1)
                    End If
                ElseIf ScrHi(I) = ScrLo(I - 1) Then
                    If (ScrLo(I) <> ScrLo(I + 1)) Then
                        ScrHi(I) = ScrLo(I)
                        ScrLo(I) = ScrLo(I - 1)
                    End If
                ElseIf ScrHi(I) = ColRAM(I + 1) Then
                    If (ColRAM(I) <> ColRAM(I - 1)) Then
                        ScrHi(I) = ColRAM(I)
                        ColRAM(I) = ColRAM(I + 1)
                    End If
                ElseIf ScrHi(I) = ColRAM(I - 1) Then
                    If (ColRAM(I) <> ColRAM(I + 1)) Then
                        ScrHi(I) = ColRAM(I)
                        ColRAM(I) = ColRAM(I - 1)
                    End If
                End If
            End If
            'Next

            'For I As Integer = 1 To ColRAM.Count - 2
            If (ScrLo(I) <> ScrLo(I - 1)) And (ScrLo(I) <> ScrLo(I + 1)) And (ScrLo(I) <> 255) Then
                If ScrLo(I) = ScrHi(I + 1) Then
                    If (ScrHi(I) <> ScrHi(I - 1)) Then
                        ScrLo(I) = ScrHi(I)
                        ScrHi(I) = ScrHi(I + 1)
                    End If
                ElseIf ScrLo(I) = ScrHi(I - 1) Then
                    If (ScrHi(I) <> ScrHi(I + 1)) Then
                        ScrLo(I) = ScrHi(I)
                        ScrHi(I) = ScrHi(I - 1)
                    End If
                ElseIf ScrLo(I) = ColRAM(I + 1) Then
                    If (ColRAM(I) <> ColRAM(I - 1)) Then
                        ScrLo(I) = ColRAM(I)
                        ColRAM(I) = ColRAM(I + 1)
                    End If
                ElseIf ScrLo(I) = ColRAM(I - 1) Then
                    If (ColRAM(I) <> ColRAM(I + 1)) Then
                        ScrLo(I) = ColRAM(I)
                        ColRAM(I) = ColRAM(I - 1)
                    End If
                End If
            End If
            'Next

            'For I As Integer = 1 To ColRAM.Count - 2
            If (ColRAM(I) <> ColRAM(I - 1)) And (ColRAM(I) <> ColRAM(I + 1)) And (ColRAM(I) <> 255) Then
                If ColRAM(I) = ScrLo(I + 1) Then
                    If (ScrLo(I) <> ScrLo(I - 1)) Then
                        ColRAM(I) = ScrLo(I)
                        ScrLo(I) = ScrLo(I + 1)
                    End If
                ElseIf ColRAM(I) = ScrLo(I - 1) Then
                    If (ScrLo(I) <> ScrLo(I + 1)) Then
                        ColRAM(I) = ScrLo(I)
                        ScrLo(I) = ScrLo(I - 1)
                    End If
                ElseIf ColRAM(I) = ScrHi(I + 1) Then
                    If (ScrHi(I) <> ScrHi(I - 1)) Then
                        ColRAM(I) = ScrHi(I)
                        ScrHi(I) = ScrHi(I + 1)
                    End If
                ElseIf ColRAM(I) = ScrHi(I - 1) Then
                    If (ScrHi(I) <> ScrHi(I + 1)) Then
                        ColRAM(I) = ScrHi(I)
                        ScrHi(I) = ScrHi(I - 1)
                    End If
                End If
            End If
        Next

        'For I As Integer = 1 To ColRAM.Count - 3
        'If (ScrHi(I) = ScrHi(I + 1)) And (ScrHi(I) <> ScrHi(I - 1)) And (ScrHi(I) <> ScrHi(I + 2)) And (ScrHi(I) <> 255) Then
        'If ScrHi(I) = ScrLo(I + 2) Then
        'If (ScrLo(I) = ScrLo(I + 1)) And (ScrLo(I) <> ScrLo(I - 1)) Then
        'ScrHi(I) = ScrLo(I)
        'ScrHi(I + 1) = ScrHi(I)
        'ScrLo(I) = ScrLo(I + 2)
        'ScrLo(I + 1) = ScrLo(I)
        'End If
        'ElseIf ScrHi(I) = ScrLo(I - 1) Then
        'If (ScrLo(I) = ScrLo(I + 1)) And (ScrLo(I) <> ScrLo(I + 2)) Then
        'ScrHi(I) = ScrLo(I)
        'ScrHi(I + 1) = ScrHi(I)
        'ScrLo(I) = ScrLo(I - 1)
        'ScrLo(I + 1) = ScrLo(I)
        'End If
        'ElseIf ScrHi(I) = ColRAM(I + 2) Then
        'If (ColRAM(I) = ColRAM(I + 1)) And (ColRAM(I) <> ColRAM(I - 1)) Then
        'ScrHi(I) = ColRAM(I)
        'ScrHi(I + 1) = ScrHi(I)
        'ColRAM(I) = ColRAM(I + 2)
        'ColRAM(I + 1) = ColRAM(I)
        'End If
        'ElseIf ScrHi(I) = ColRAM(I - 1) Then
        'If (ColRAM(I) = ColRAM(I + 1)) And (ColRAM(I) <> ColRAM(I + 2)) Then
        'ScrHi(I) = ColRAM(I)
        'ScrHi(I + 1) = ScrHi(I)
        'ColRAM(I) = ColRAM(I - 1)
        'ColRAM(I + 1) = ColRAM(I)
        'End If
        'End If
        'End If

        'If (ScrLo(I) = ScrLo(I + 1)) And (ScrLo(I) <> ScrLo(I - 1)) And (ScrLo(I) <> ScrLo(I + 2)) And (ScrLo(I) <> 255) Then
        'If ScrLo(I) = ScrHi(I + 2) Then
        'If (ScrHi(I) = ScrHi(I + 1)) And (ScrHi(I) <> ScrHi(I - 1)) Then
        'ScrLo(I) = ScrHi(I)
        'ScrLo(I + 1) = ScrLo(I)
        'ScrHi(I) = ScrHi(I + 2)
        'ScrHi(I + 1) = ScrHi(I)
        'End If
        'ElseIf ScrLo(I) = Scrhi(I - 1) Then
        'If (ScrHi(I) = ScrHi(I + 1)) And (ScrHi(I) <> ScrHi(I + 2)) Then
        'ScrLo(I) = ScrHi(I)
        'ScrLo(I + 1) = ScrLo(I)
        'ScrHi(I) = ScrHi(I - 1)
        'ScrHi(I + 1) = ScrHi(I)
        'End If
        'ElseIf Scrlo(I) = ColRAM(I + 2) Then
        'If (ColRAM(I) = ColRAM(I + 1)) And (ColRAM(I) <> ColRAM(I - 1)) Then
        'ScrLo(I) = ColRAM(I)
        'ScrLo(I + 1) = ScrLo(I)
        'ColRAM(I) = ColRAM(I + 2)
        'ColRAM(I + 1) = ColRAM(I)
        'End If
        'ElseIf Scrlo(I) = ColRAM(I - 1) Then
        'If (ColRAM(I) = ColRAM(I + 1)) And (ColRAM(I) <> ColRAM(I + 2)) Then
        'ScrLo(I) = ColRAM(I)
        'ScrLo(I + 1) = ScrLo(I)
        'ColRAM(I) = ColRAM(I - 1)
        'ColRAM(I + 1) = ColRAM(I)
        'End If
        ''End If
        'End If

        'If (ColRAM(I) = ColRAM(I + 1)) And (ColRAM(I) <> ColRAM(I - 1)) And (ColRAM(I) <> ColRAM(I + 2)) And (ColRAM(I) <> 255) Then
        'If ColRAM(I) = ScrLo(I + 2) Then
        'If (ScrLo(I) = ScrLo(I + 1)) And (ScrLo(I) <> ScrLo(I - 1)) Then
        'ColRAM(I) = ScrLo(I)
        'ColRAM(I + 1) = ColRAM(I)
        'ScrLo(I) = ScrLo(I + 2)
        'ScrLo(I + 1) = ScrLo(I)
        'End If
        'ElseIf ColRAM(I) = ScrLo(I - 1) Then
        'If (ScrLo(I) = ScrLo(I + 1)) And (ScrLo(I) <> ScrLo(I + 2)) Then
        'ColRAM(I) = ScrLo(I)
        'ColRAM(I + 1) = ColRAM(I)
        'ScrLo(I) = ScrLo(I - 1)
        'ScrLo(I + 1) = ScrLo(I)
        'End If
        'ElseIf ColRAM(I) = ScrHi(I + 2) Then
        'If (ScrHi(I) = ScrHi(I + 1)) And (ScrHi(I) <> ScrHi(I - 1)) Then
        'ColRAM(I) = ScrHi(I)
        'ColRAM(I + 1) = ColRAM(I)
        'ScrHi(I) = ScrHi(I + 2)
        'ScrHi(I + 1) = ScrHi(I)
        'End If
        'ElseIf ColRAM(I) = ScrHi(I - 1) Then
        'If (ScrHi(I) = ScrHi(I + 1)) And (ScrHi(I) <> ScrHi(I + 2)) Then
        'ColRAM(I) = ScrHi(I)
        'ColRAM(I + 1) = ColRAM(I)
        'ScrHi(I) = ScrHi(I - 1)
        'ScrHi(I + 1) = ScrHi(I)
        'End If
        'End If
        'End If
        'Next

        If CmdLn = False Then
            FrmSPOT.TSSL.Text = "Filling unused space..."
            FrmSPOT.Refresh()
        End If

        If ColRAM(0) = 255 Then
            For I As Integer = 1 To ColRAM.Count - 1
                If ColRAM(I) <> 255 Then
                    ColRAM(0) = ColRAM(I)
                    Exit For
                End If
            Next
            If ColRAM(0) = 255 Then ColRAM(0) = 0 'In case the whole array remained unused
        End If

            If ScrHi(0) = 255 Then
            For I As Integer = 1 To ScrHi.Count - 1
                If ScrHi(I) <> 255 Then
                    ScrHi(0) = ScrHi(I)
                    Exit For
                End If
            Next
            If ScrHi(0) = 255 Then ScrHi(0) = 0 'In case the whole array remained unused
        End If

        If ScrLo(0) = 255 Then
            For I As Integer = 1 To ScrLo.Count - 1
                If ScrLo(I) <> 255 Then
                    ScrLo(0) = ScrLo(I)
                    Exit For
                End If
            Next
            If ScrLo(0) = 255 Then ScrLo(0) = 0 'In case the whole array remained unused
        End If

        For I As Integer = 1 To ColRAM.Count - 1
            If ScrHi(I) = 255 Then
                'Blank0(I) = 255
                ScrHi(I) = ScrHi(I - 1)
            End If

            If (ScrLo(I) = ScrHi(I)) Or (ScrLo(I) = 255) Then
                'Blank1(I) = 255
                ScrLo(I) = ScrLo(I - 1)
            End If

            If (ColRAM(I) = ScrHi(I)) Or (ColRAM(I) = ScrLo(I)) Or (ColRAM(I) = 255) Then
                'Blank2(I) = 255
                ColRAM(I) = ColRAM(I - 1)
            End If
        Next

        'For I As Integer = 1 To ColRAM.Count - 2
        'If ((ScrHi(I) <> ScrHi(I - 1)) And (ScrHi(I) <> ScrHi(I + 1))) Or
        '((ScrLo(I) <> ScrLo(I - 1)) And (ScrLo(I) <> ScrLo(I + 1))) Or
        '((ColRAM(I) <> ColRAM(I - 1)) And (ColRAM(I) <> ColRAM(I + 1))) Then
        'Dim C1, C2, C3, N1, N2, N3, P1, P2, P3 As Byte
        'C1 = ScrHi(I)
        'N1 = ScrHi(I + 1)
        'P1 = ScrHi(I - 1)
        'C2 = ScrLo(I)
        'N2 = ScrLo(I + 1)
        'P2 = ScrLo(I - 1)
        'C3 = ColRAM(I)
        'N3 = ColRAM(I + 1)
        'P3 = ColRAM(I - 1)
        'If ((C1 = N2) And (C2 = N3)) Or ((C1 = N2) And (C3 = N1)) Then
        'ScrHi(I) = C3
        'ScrLo(I) = C1
        'ColRAM(I) = C2
        'ElseIf (C1 = N2) And (C2 = N1) Then
        'ScrHi(I) = C2
        'ScrLo(I) = C1
        ''ColRAM(I) = c3
        'ElseIf ((C1 = N3) And (C2 = N1)) Or ((C1 = N3) And (C3 = N2)) Then
        'ScrHi(I) = C2
        'ScrLo(I) = C3
        'ColRAM(I) = C1
        'ElseIf (C1 = N3) And (C3 = N1) Then
        'ScrHi(I) = C3
        ''ScrLo(I) = C2
        'ColRAM(I) = C1
        'ElseIf ((C1 = P2) And (C2 = P3)) Or ((C1 = P2) And (C3 = P1)) Then
        'ScrHi(I) = C3
        'ScrLo(I) = C1
        'ColRAM(I) = C2
        'ElseIf (C1 = P2) And (C2 = P1) Then
        'ScrHi(I) = C2
        'ScrLo(I) = C1
        ''ColRAM(I) = C3
        'ElseIf ((C1 = P3) And (C2 = P1)) Or ((C1 = P3) And (C3 = P2)) Then
        'ScrHi(I) = C2
        'ScrLo(I) = C3
        'ColRAM(I) = C1
        'ElseIf (C1 = P3) And (C3 = P1) Then
        'ScrHi(I) = C3
        ''ScrLo(I) = C2
        'ColRAM(I) = C1
        'End If
        'End If
        'Next

        'RetroArrangeBitmap()

        If CmdLn = False Then
            FrmSPOT.TSSL.Text = "Preparing screen RAM..."
            FrmSPOT.Refresh()
        End If

        'Combine screen RAM hi and low nibbles
        For I As Integer = 0 To ScrRAM.Count - 1
            Dim Tmp As Byte = ColRAM(I)
            ColRAM(I) = ScrLo(I)                    'Swab ColRAM with ScrLo - this results in improved compressibility
            ScrLo(I) = Tmp
            ScrRAM(I) = ((ScrHi(I) Mod 16) * 16) + (ScrLo(I) Mod 16)
        Next

        'IO.File.WriteAllBytes(SpotFolder + "\ColRAM.bin", ColRAM)
        'IO.File.WriteAllBytes(SpotFolder + "\ScrHi.bin", ScrHi)
        'IO.File.WriteAllBytes(SpotFolder + "\ScrLo.bin", ScrLo)
        'IO.File.WriteAllBytes(SpotFolder + "\ScrRAM.bin", ScrRAM)

        '----------------------------------------------------------------------------
        'Rebuild the image
        '----------------------------------------------------------------------------

        If CmdLn = False Then
            FrmSPOT.TSSL.Text = "Rebuilding the bitmap..."
            FrmSPOT.Refresh()
        End If

        Dim Col1, Col2, Col3 As Byte
        Dim CP As Integer   'Char Position within array
        Dim V As Byte       'One byte to work with...

        'Replace C64 colors with respective bit pairs
        For CY As Integer = 0 To CharRow - 1
            For CX As Integer = 0 To CharCol - 1
                Col1 = ScrHi((CY * CharCol) + CX)        'Fetch colors from tabs
                Col2 = ScrLo((CY * CharCol) + CX)
                Col3 = ColRAM((CY * CharCol) + CX)
                For BY As Integer = 0 To 7
                    For BX As Integer = 0 To 3
                        'Calculate pixel position in array
                        CP = (CY * PicW * 8) + (CX * 4) + (BY * PicW) + BX
                        If Pic(CP) = BGCol Then
                            PicMsk(CP) = 0
                        ElseIf Pic(CP) = Col1 Then
                            PicMsk(CP) = 1
                        ElseIf Pic(CP) = Col2 Then
                            PicMsk(CP) = 2
                        ElseIf Pic(CP) = Col3 Then
                            PicMsk(CP) = 3
                        End If
                    Next
                Next
            Next
        Next

        'Finally, convert bit pairs to final bitmap
        For CY As Integer = 0 To CharRow - 1
            For CX As Integer = 0 To CharCol - 1
                For BY As Integer = 0 To 7
                    CP = (CY * PicW * 8) + (CX * 4) + (BY * PicW)
                    V = (PicMsk(CP) * 64) + (PicMsk(CP + 1) * 16) + (PicMsk(CP + 2) * 4) + PicMsk(CP + 3)
                    CP = (CY * CharCol * 8) + (CX * 8) + BY
                    BMP(CP) = V
                Next
            Next
        Next

        If Not Directory.Exists(SavePath) Then
            Directory.CreateDirectory(SavePath)
        End If

        If ((CmdLn = False) And (My.Settings.OutputKla = True)) Or (InStr(CmdOptions, "k") <> 0) Then
            'Save Koala only if bitmap is at least 320x200 pixels
            If (CharRow >= 25) And (CharCol >= 40) Then

                Dim Koala(10002) As Byte
                Koala(1) = &H60
                Koala(10002) = BGCol

                Dim StartCX, StartCY, StartBY As Integer

                StartCX = Int(CharCol / 2) - 20
                StartCY = Int(CharRow / 2) - 12
                StartBY = (CharCol * 4) - 160

                For CY As Integer = 0 To 24
                    For BY As Integer = 0 To 319
                        Koala((CY * 320) + BY + 2) = BMP(((StartCY + CY) * CharCol * 8) + StartBY + BY)
                    Next
                Next

                For CY As Integer = 0 To 24
                    For CX As Integer = 0 To 39
                        Koala(8002 + (CY * 40) + CX) = ScrRAM(((StartCY + CY) * CharCol) + StartCX + CX)
                        Koala(9002 + (CY * 40) + CX) = ColRAM(((StartCY + CY) * CharCol) + StartCX + CX)
                    Next
                Next

                If CmdLn = False Then
                    FrmSPOT.TSSL.Text = "Saving Koala..."
                    FrmSPOT.Refresh()
                End If

                'Save Koala
                File.WriteAllBytes(SavePath + "\" + SaveName + "_0" + Hex(BGCol) + ".kla", Koala)
            End If
        End If

        'Save bitmap, color RAM, and screen RAM

        If ((CmdLn = False) And (My.Settings.OutputMap = True)) Or (InStr(CmdOptions, "m") <> 0) Then
            If CmdLn = False Then
                FrmSPOT.TSSL.Text = "Saving MAP file..."
                FrmSPOT.Refresh()
            End If
            File.WriteAllBytes(SavePath + "\" + SaveName + "_0" + Hex(BGCol) + ".map", BMP)
        End If

        If ((CmdLn = False) And (My.Settings.OutputCol = True)) Or (InStr(CmdOptions, "c") <> 0) Then
            If CmdLn = False Then
                FrmSPOT.TSSL.Text = "Saving COL file..."
                FrmSPOT.Refresh()
            End If
            File.WriteAllBytes(SavePath + "\" + SaveName + "_0" + Hex(BGCol) + ".col", ColRAM)
        End If

        If ((CmdLn = False) And (My.Settings.OutputScr = True)) Or (InStr(CmdOptions, "s") <> 0) Then
            If CmdLn = False Then
                FrmSPOT.TSSL.Text = "Saving SCR file..."
                FrmSPOT.Refresh()
            End If

            File.WriteAllBytes(SavePath + "\" + SaveName + "_0" + Hex(BGCol) + ".scr", ScrRAM)
        End If

        Dim CCR((ColRAM.Count / 2) - 1) As Byte

        For I As Integer = 0 To (ColRAM.Count / 2) - 1
            CCR(I) = ((ColRAM(I * 2) Mod 16) * 16) + (ColRAM((I * 2) + 1) Mod 16)
        Next

        'Save compressed ColorRAM with halfbytes combined
        If ((CmdLn = False) And (My.Settings.OutputCcr = True)) Or (InStr(CmdOptions, "2") <> 0) Then
            If CmdLn = False Then
                FrmSPOT.TSSL.Text = "Saving CCR file..."
                FrmSPOT.Refresh()
            End If
            File.WriteAllBytes(SavePath + "\" + SaveName + "_0" + Hex(BGCol) + ".ccr", CCR)
        End If

        'Save Optimized Bitmap file format only if bitmap is at least 320x200 pixels
        'Bitmap is stored column wise, color spaces are stored row wise, color RAM is compressed
        If ((CmdLn = False) And (My.Settings.OutputObm = True)) Or (InStr(CmdOptions, "o") <> 0) Then
            If (CharRow >= 25) And (CharCol >= 40) Then
                If CmdLn = False Then
                    FrmSPOT.TSSL.Text = "Saving OBM file..."
                    FrmSPOT.Refresh()
                End If
                Dim OBM(9502) As Byte
                OBM(1) = &H60
                OBM(9502) = BGCol

                Dim StartCX, StartCY, StartBY As Integer

                StartCX = Int(CharCol / 2) - 20
                StartCY = Int(CharRow / 2) - 12
                StartBY = (CharCol * 4) - 160

                'Bitmap stored column wise
                For CX As Integer = 0 To 39
                    For CY As Integer = 0 To 24
                        For BY As Integer = 0 To 7
                            OBM((CX * 200) + (CY * 8) + BY + 2) = BMP(((StartCY + CY) * CharCol * 8) + ((StartCX + CX) * 8) + BY)
                        Next
                    Next
                Next

                'Screen RAM stored row wise
                For CY As Integer = 0 To 24
                    For CX As Integer = 0 To 39
                        OBM(8002 + (CY * 40) + CX) = ScrRAM(((StartCY + CY) * CharCol) + StartCX + CX)
                    Next
                Next

                StartCX = Int(StartCX / 2)

                'Compressed Color RAM stored row wise
                For CY As Integer = 0 To 24
                    For CX As Integer = 0 To 19
                        OBM(9002 + (CY * 20) + CX) = CCR(((StartCY + CY) * Int(CharCol / 2)) + StartCX + CX)
                    Next
                Next

                'Save optimized bitmap file format
                File.WriteAllBytes(SavePath + "\" + SaveName + "_0" + Hex(BGCol) + ".obm", OBM)
            End If
        End If

        Exit Sub

Err:
        MsgBox(ErrorToString(), vbOKOnly + vbExclamation, Reflection.MethodBase.GetCurrentMethod.Name + " Error")

    End Sub

    Public Sub SaveImgFormat()
        On Error GoTo Err

        If Not Directory.Exists(SavePath) Then
            Directory.CreateDirectory(SavePath)
        End If

        If ((CmdLn = False) And (My.Settings.OutputPng = True)) Or (InStr(CmdOptions, "p") <> 0) Then
            If LCase(SavePath + "\" + SaveName + ".png") <> LCase(InputFilePath) Then
                If CmdLn = False Then
                    FrmSPOT.TSSL.Text = "Saving PNG file..."
                    FrmSPOT.Refresh()
                End If
                FrmSPOT.PbxPic.Image.Save(SavePath + "\" + SaveName + ".png", Imaging.ImageFormat.Png)
            Else
                MsgBox("Unable to save PNG format as it would overwrite the original file!", vbOKOnly + vbInformation, "PNG cannot be created")
            End If
        End If

        If ((CmdLn = False) And (My.Settings.OutputBmp = True)) Or (InStr(CmdOptions, "b") <> 0) Then
            If LCase(SavePath + "\" + SaveName + ".bmp") <> LCase(InputFilePath) Then
                If CmdLn = False Then
                    FrmSPOT.TSSL.Text = "Saving BMP file..."
                    FrmSPOT.Refresh()
                End If
                FrmSPOT.PbxPic.Image.Save(SavePath + "\" + SaveName + ".bmp", Imaging.ImageFormat.Bmp)
            Else
                MsgBox("Unable to save BMP format as it would overwrite the original file!", vbOKOnly + vbInformation, "BMP cannot be created")
            End If
        End If

        If ((CmdLn = False) And (My.Settings.OutputJpg = True)) Or (InStr(CmdOptions, "j") <> 0) Then
            If LCase(SavePath + "\" + SaveName + ".jpg") <> LCase(InputFilePath) Then
                If CmdLn = False Then
                    FrmSPOT.TSSL.Text = "Saving JPG file..."
                    FrmSPOT.Refresh()
                End If
                FrmSPOT.PbxPic.Image.Save(SavePath + "\" + SaveName + ".jpg", Imaging.ImageFormat.Jpeg)
            Else
                MsgBox("Unable to save JPG format as it would overwrite the original file!", vbOKOnly + vbInformation, "JPG cannot be created")
            End If
        End If

        Exit Sub

Err:
        MsgBox(ErrorToString(), vbOKOnly + vbExclamation, Reflection.MethodBase.GetCurrentMethod.Name + " Error")

    End Sub

    Public Sub ResizePbx()
        On Error GoTo Err

        With FrmSPOT.Pnl
            If .AutoScroll = True Then
                ScrollPos.X = Math.Abs(.AutoScrollPosition.X)
                ScrollPos.Y = Math.Abs(.AutoScrollPosition.Y)
                .AutoScroll = False
            Else
                ScrollPos.X = -1
                ScrollPos.Y = -1
            End If
        End With

        With FrmSPOT.PbxPic
            .Width = (PicW * 4) '- If((PicW = Int(FrmSPOT.Pnl.Width / 4)) And (PicH > Int(FrmSPOT.Pnl.Height / 2)), 18, 0)
            .Height = (PicH * 2) ' - If((PicW > Int(FrmSPOT.Pnl.Width / 4)) And (PicH = Int(FrmSPOT.Pnl.Height / 2)), 18, 0)
            .Left = If(.Width > FrmSPOT.Pnl.Width, 0, Int((FrmSPOT.Pnl.Width - .Width) / 2))
            .Top = If(.Height > FrmSPOT.Pnl.Height, 0, Int((FrmSPOT.Pnl.Height - .Height) / 2))
        End With

        With FrmSPOT.PbxAC
            .Width = (PicW * 4) '- If((PicW = Int(FrmSPOT.Pnl.Width / 4)) And (PicH > Int(FrmSPOT.Pnl.Height / 2)), 18, 0)
            .Height = (PicH * 2) ' - If((PicW > Int(FrmSPOT.Pnl.Width / 4)) And (PicH = Int(FrmSPOT.Pnl.Height / 2)), 18, 0)
            .Left = If(.Width > FrmSPOT.Pnl.Width, 0, Int((FrmSPOT.Pnl.Width - .Width) / 2))
            .Top = If(.Height > FrmSPOT.Pnl.Height, 0, Int((FrmSPOT.Pnl.Height - .Height) / 2))
        End With

        With FrmSPOT.PbxSH
            .Width = (PicW * 4) '- If((PicW = Int(FrmSPOT.Pnl.Width / 4)) And (PicH > Int(FrmSPOT.Pnl.Height / 2)), 18, 0)
            .Height = (PicH * 2) '- If((PicW > Int(FrmSPOT.Pnl.Width / 4)) And (PicH = Int(FrmSPOT.Pnl.Height / 2)), 18, 0)
            .Left = If(.Width > FrmSPOT.Pnl.Width, 0, Int((FrmSPOT.Pnl.Width - .Width) / 2))
            .Top = If(.Height > FrmSPOT.Pnl.Height, 0, Int((FrmSPOT.Pnl.Height - .Height) / 2))
        End With

        With FrmSPOT.PbxSL
            .Width = (PicW * 4) '- If((PicW = Int(FrmSPOT.Pnl.Width / 4)) And (PicH > Int(FrmSPOT.Pnl.Height / 2)), 18, 0)
            .Height = (PicH * 2) ' - If((PicW > Int(FrmSPOT.Pnl.Width / 4)) And (PicH = Int(FrmSPOT.Pnl.Height / 2)), 18, 0)
            .Left = If(.Width > FrmSPOT.Pnl.Width, 0, Int((FrmSPOT.Pnl.Width - .Width) / 2))
            .Top = If(.Height > FrmSPOT.Pnl.Height, 0, Int((FrmSPOT.Pnl.Height - .Height) / 2))
        End With

        With FrmSPOT.PbxCR
            .Width = (PicW * 4) '- If((PicW = Int(FrmSPOT.Pnl.Width / 4)) And (PicH > Int(FrmSPOT.Pnl.Height / 2)), 18, 0)
            .Height = (PicH * 2) '- If((PicW > Int(FrmSPOT.Pnl.Width / 4)) And (PicH = Int(FrmSPOT.Pnl.Height / 2)), 18, 0)
            .Left = If(.Width > FrmSPOT.Pnl.Width, 0, Int((FrmSPOT.Pnl.Width - .Width) / 2))
            .Top = If(.Height > FrmSPOT.Pnl.Height, 0, Int((FrmSPOT.Pnl.Height - .Height) / 2))
            If (.Width > FrmSPOT.Pnl.Width) Or (.Height > FrmSPOT.Pnl.Height) Then
                If ScrollPos.X = -1 Then
                    If .Width > FrmSPOT.Pnl.Width Then
                        ScrollPos.X = Int((.Width - FrmSPOT.Pnl.Width) / 2)
                    Else
                        ScrollPos.X = 0
                    End If
                End If
                If ScrollPos.Y = -1 Then
                    If .Height > FrmSPOT.Pnl.Height Then
                        ScrollPos.Y = Int((.Height - FrmSPOT.Pnl.Height) / 2)
                    Else
                        ScrollPos.Y = 0
                    End If
                End If
                FrmSPOT.Pnl.AutoScroll = True
                FrmSPOT.Pnl.AutoScrollPosition = ScrollPos
            End If
        End With

        Exit Sub

Err:
        MsgBox(ErrorToString(), vbOKOnly + vbExclamation, Reflection.MethodBase.GetCurrentMethod.Name + " Error")
    End Sub

    Private Sub RearrangeBitmap()
        On Error GoTo Err

        If (PicW <> 2240) Or (PicH <> 1800) Then Exit Sub

        Dim PX1, PY1, PX2, PY2 As Integer
        Dim C1, C2 As Color

        For CY As Integer = 0 To CharRow - 26
            PX1 = ((CharRow - CY - 1) * 2 * 8) + 320
            PY1 = CY * 8
            PX2 = PX1 - 16
            PY2 = PY1 + 8
            For I As Integer = 0 To 23
                For BY As Integer = 0 To 7
                    For BX As Integer = 0 To 15
                        C1 = C64Bitmap.GetPixel(PX1 + BX, PY1 + BY)
                        C2 = C64Bitmap.GetPixel(PX2 + BX, PY2 + BY)
                        C64Bitmap.SetPixel(PX1 + BX, PY1 + BY, C2)
                        C64Bitmap.SetPixel(PX2 + BX, PY2 + BY, C1)
                    Next
                Next
                PX1 += 16
                PY2 += 8
            Next
        Next

        Exit Sub

Err:
        MsgBox(ErrorToString(), vbOKOnly + vbExclamation, Reflection.MethodBase.GetCurrentMethod.Name + " Error")

    End Sub

    Private Sub RetroArrangeBitmap()
        On Error GoTo Err

        If (PicW <> 2240) Or (PicH <> 1800) Then Exit Sub

        Dim PX1, PY1, PX2, PY2 As Integer
        Dim V1, V2 As Byte
        Dim C1 As Color
        For CY As Integer = CharRow - 26 To 0 Step -1
            PX1 = ((CharRow - CY - 1) * 2 * 4) + 160
            PY1 = CY * 8
            PX2 = PX1 - 8
            PY2 = PY1 + 8
            For I As Integer = 23 To 0 Step -1
                For BY As Integer = 0 To 7
                    For BX As Integer = 0 To 7
                        V1 = Pic(((PY1 + BY) * PicW) + PX1 + BX)
                        V2 = Pic(((PY2 + BY) * PicW) + PX2 + BX)
                        Pic(((PY1 + BY) * PicW) + PX1 + BX) = V2
                        Pic(((PY2 + BY) * PicW) + PX2 + BX) = V1
                    Next
                Next
                PX1 += 8
                PY2 += 8
            Next
        Next

        For Y As Integer = 0 To PicH - 1
            For X As Integer = 0 To PicW - 1
                V1 = Pic((Y * PicW) + X)
                C1 = Color.FromArgb(C64Palettes(V1), C64Palettes(V1 + 16), C64Palettes(V1 + 32))
                OrigBitmap.SetPixel((X * 2), Y, C1)
                OrigBitmap.SetPixel((X * 2) + 1, Y, C1)
                FrmSPOT.PbxPic.Image = OrigBitmap
                'PbxPic.Refresh()
            Next
        Next

        For CY As Integer = CharRow - 26 To 0 Step -1
            PX1 = ((CharRow - CY - 1) * 2) + 40
            PY1 = CY
            PX2 = PX1 - 2
            PY2 = PY1 + 1
            For I As Integer = 23 To 0 Step -1
                For BX As Integer = 0 To 1
                    V1 = ColRAM((PY1 * CharCol) + PX1 + BX)
                    V2 = ColRAM((PY2 * CharCol) + PX2 + BX)
                    ColRAM((PY1 * CharCol) + PX1 + BX) = V2
                    ColRAM((PY2 * CharCol) + PX2 + BX) = V1
                    V1 = ScrLo((PY1 * CharCol) + PX1 + BX)
                    V2 = ScrLo((PY2 * CharCol) + PX2 + BX)
                    ScrLo((PY1 * CharCol) + PX1 + BX) = V2
                    ScrLo((PY2 * CharCol) + PX2 + BX) = V1
                    V1 = ScrHi((PY1 * CharCol) + PX1 + BX)
                    V2 = ScrHi((PY2 * CharCol) + PX2 + BX)
                    ScrHi((PY1 * CharCol) + PX1 + BX) = V2
                    ScrHi((PY2 * CharCol) + PX2 + BX) = V1
                Next
                PX1 += 2
                PY2 += 1
            Next
        Next

        Exit Sub

Err:
        MsgBox(ErrorToString(), vbOKOnly + vbExclamation, Reflection.MethodBase.GetCurrentMethod.Name + " Error")

    End Sub

End Module
