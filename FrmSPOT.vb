Imports System.IO

Public Class FrmSPOT
    Private Sub Main_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        On Error GoTo Err

        Dim Splash As New FrmSplash
        Dim FInfo, OutInfo As FileInfo
        CmdArg = Environment.GetCommandLineArgs()

        If CmdArg.Length > 1 Then

            Splash.Show()

            CmdLn = True

            InputFilePath = LCase(CmdArg(1))                        'Input file path and name

            CmdOptions = ""
            CmdOut = ""

            If CmdArg.Length > 2 Then
                CmdOptions = LCase(CmdArg(2))                       'Command line options
            End If

            If CmdArg.Length > 3 Then
                CmdOut = LCase(CmdArg(3))
            End If

            If CmdArg.Length > 4 Then
                CmdColors = LCase(CmdArg(4))
            Else
                CmdColors = "0123456789abcdef"                      'If argument is not specified, we allow any background colors
            End If

            If CmdOptions = "" Then
                With My.Settings
                    If .OutputKla Then CmdOptions += "k"
                    If .OutputMap Then CmdOptions += "m"
                    If .OutputCol Then CmdOptions += "c"
                    If .OutputScr Then CmdOptions += "s"
                    If .OutputCcr Then CmdOptions += "2"
                    If .OutputObm Then CmdOptions += "o"
                    If .OutputPng Then CmdOptions += "p"
                    If .OutputBmp Then CmdOptions += "b"
                    If .OutputJpg Then CmdOptions += "j"
                End With
            End If

            FInfo = My.Computer.FileSystem.GetFileInfo(InputFilePath)
            FPath = FInfo.DirectoryName
            FName = FInfo.Name                                           'Strings.Replace(FInfo.Name, FExt, "")
            FExt = FInfo.Extension
            InputFilePath = FPath + "\" + FName                          'Create FULL path
            If FExt <> "" Then
                FName = Strings.Left(FName, Len(FName) - Len(FExt))
            End If
            SpotFolder = FPath + "\SPOT"

            If CmdOut <> "" Then
                'If an outfile path and name is entered, use them
                OutInfo = My.Computer.FileSystem.GetFileInfo(CmdOut)
                SavePath = OutInfo.DirectoryName
                SaveName = OutInfo.Name
                SaveExt = OutInfo.Extension
                If SaveExt <> "" Then
                    SaveName = Strings.Left(SaveName, Len(SaveName) - Len(SaveExt))
                End If
            Else
                'If no outfile specified, use the SPOT folder and the infile's name
                SavePath = SpotFolder + "\" + FName
                SaveName = FName
            End If

            'MsgBox(OutInfo.DirectoryName + vbNewLine + OutInfo.Name + vbNewLine + "'" + OutInfo.Extension + "'")

            If File.Exists(InputFilePath) = False Then
                MsgBox("Unable to open the following file:" + vbNewLine + vbNewLine + InputFilePath, vbOKOnly + vbExclamation, "Error opening file")
            Else
                Cursor = Cursors.WaitCursor

                    Select Case LCase(FExt)
                        Case ".kla", ".koa"
                        KLA = File.ReadAllBytes(InputFilePath)
                        If KLA.Length <> 10003 Then
                                MsgBox("Can't optimize this Koala file", vbOKOnly + vbCritical, "Invalid Koala file")
                                Console.WriteLine("Picture optimization unsuccessful")
                                GoTo ExitNoErr
                            End If

                            FromKla = True
                            OptimizeKla()

                        Case ".png", ".bmp"
                        OrigBitmap = New Bitmap(InputFilePath)

                        FromKla = False
                            ConvertPicToC64Palette()
                            OptimizePng()

                        Case Else
                            MsgBox("File format is not supported!" + vbNewLine + vbNewLine +
                               "SPOT accepts PNG, BMP, and KLA for input", vbOKOnly + vbInformation, "SPOT")
                    End Select

                    Cursor = Cursors.Default

                End If

                Splash.Close()

ExitNoErr:      Close()                                 'This will close the main form and set the exit code and exit Sparkle

                Exit Sub                                'This is needed to prevent Error Chime on exit
            End If

            TSSL.Text = "No file selected"

        Text = "SPOT - Sparta's Picture Optimizing Tool for the Commodore 64 v" + My.Application.Info.Version.Major.ToString + "." + My.Application.Info.Version.Minor.ToString +
                "." + My.Application.Info.Version.Build.ToString + "." + If(Len(My.Application.Info.Version.Revision.ToString) = 3, "0", "") +
                My.Application.Info.Version.Revision.ToString ' + ")"

        Exit Sub

Err:
        Splash.Close()
        MsgBox(ErrorToString(), vbOKOnly + vbExclamation, Reflection.MethodBase.GetCurrentMethod.Name + " Error")
        Cursor = Cursors.Default

        If CmdArg.Length > 1 Then Close()

    End Sub

    Private Sub BtnSettings_Click(sender As Object, e As EventArgs) Handles BtnSettings.Click
        On Error GoTo Err

        Using Frm As New FrmSettings
            With Frm
                .ShowDialog(Me)
            End With
        End Using

        Exit Sub

Err:
        MsgBox(ErrorToString(), vbOKOnly + vbExclamation, Reflection.MethodBase.GetCurrentMethod.Name + " Error")

    End Sub

    Private Sub BtnOptimize_Click(sender As Object, e As EventArgs) Handles BtnOptimize.Click
        On Error GoTo Err

        Dim FInfo As FileInfo
        Dim OpenDLG As New OpenFileDialog

        With OpenDLG
            .Filter = "Optimizable files(*.bmp; *.png; *.kla)|*.bmp; *.png; *.kla|Image files (*.bmp; *.png)|*.bmp; *.png; |Koala files (*.kla)|*.kla|All files (*.*)|*.*"
            .Title = "Open File"
            .RestoreDirectory = True

            DialogResult = OpenDLG.ShowDialog(Me)

            If DialogResult = DialogResult.OK Then
                If File.Exists(.FileName) = False Then
                    MsgBox("Unable to open the following file:" + vbNewLine + vbNewLine + .FileName, vbOKOnly + vbExclamation, "Error opening file")
                    Exit Sub
                Else
                    InputFilePath = .FileName
                    FInfo = My.Computer.FileSystem.GetFileInfo(InputFilePath)
                    FPath = FInfo.DirectoryName
                    FExt = FInfo.Extension
                    FName = FInfo.Name 'Strings.Replace(FInfo.Name, FExt, "")
                    InputFilePath = FPath + "\" + FName
                    If FExt <> "" Then
                        FName = Strings.Left(FName, Len(FName) - Len(FExt))
                    End If
                    SpotFolder = FPath + "\SPOT"
                End If
            Else
                Exit Sub
            End If
        End With

        Cursor = Cursors.WaitCursor
        Dim Success As Boolean = False
        Select Case LCase(FExt)
            Case ".kla", ".koa"
                KLA = File.ReadAllBytes(InputFilePath)
                If KLA.Length <> 10003 Then
                    MsgBox("Can't optimize this Koala file", vbOKOnly + vbCritical, "Invalid Koala file")
                    Exit Sub
                End If

                FromKla = True
                If OptimizeKla() = False Then GoTo NoConv

            Case ".png", ".bmp"
                OrigBitmap = New Bitmap(InputFilePath)

                FromKla = False
                If ConvertPicToC64Palette() = False Then GoTo NoConv
                If OptimizePng() = False Then GoTo NoConv

            Case Else
                MsgBox("This file format is not recognized!", vbOKOnly + vbInformation, "File not recognized")
        End Select

        Cursor = Cursors.Default

        TSSL.Text = "Conversion complete: " + InputFilePath
        Refresh()

        Exit Sub

Err:
        MsgBox(ErrorToString(), vbOKOnly + vbExclamation, Reflection.MethodBase.GetCurrentMethod.Name + " Error")
NoConv:
        TSSL.Text = "Image could not be converted :("
        Refresh()
        Cursor = Cursors.Default

    End Sub

    Private Sub PbxPic_MouseDown(sender As Object, e As MouseEventArgs) Handles PbxPic.MouseDown
        On Error GoTo Err

        Select Case e.Button
            Case MouseButtons.Left
                PbxPic.Image = OrigBitmap
                PbxPic.Refresh()
            Case Else
        End Select

        Exit Sub

Err:
        MsgBox(ErrorToString(), vbOKOnly + vbExclamation, Reflection.MethodBase.GetCurrentMethod.Name + " Error")

    End Sub

    Private Sub PbxPic_MouseUp(sender As Object, e As MouseEventArgs) Handles PbxPic.MouseUp
        On Error GoTo Err

        PbxPic.Image = C64Bitmap
        PbxPic.Refresh()

        Exit Sub

Err:
        MsgBox(ErrorToString(), vbOKOnly + vbExclamation, Reflection.MethodBase.GetCurrentMethod.Name + " Error")

    End Sub

    Private Sub BtnLoadKla_Click(sender As Object, e As EventArgs) Handles BtnLoadKla.Click
        On Error GoTo Err

        Dim FInfo As FileInfo
        Dim OpenDLG As New OpenFileDialog
        Dim kla() As Byte
        Dim ColRAMCol(15) As Integer, ScrHiCol(15) As Integer, ScrLoCol(15) As Integer
        With OpenDLG
            .Filter = "Koala files (*.kla)|*.kla|All files (*.*)|*.*"
            .Title = "Open File"
            .RestoreDirectory = True

            DialogResult = OpenDLG.ShowDialog(Me)

            If DialogResult = DialogResult.OK Then
                If File.Exists(.FileName) = False Then
                    MsgBox("Unable to open the following file:" + vbNewLine + vbNewLine + .FileName, vbOKOnly + vbExclamation, "Error opening file")
                    Exit Sub
                Else
                    InputFilePath = .FileName

                    FInfo = My.Computer.FileSystem.GetFileInfo(InputFilePath)
                    FPath = FInfo.DirectoryName
                    FExt = FInfo.Extension
                    FName = Strings.Replace(FInfo.Name, FExt, "")
                    SpotFolder = FPath + "\SPOT"
                End If
            Else
                Exit Sub
            End If

            kla = File.ReadAllBytes(.FileName)
        End With

        If (kla.Count <> 10003) Or (kla(0) <> 0) Or (kla(1) <> &H60) Then
            MsgBox("Hmm... This does not appear to be a Koala image...", vbOKOnly + vbExclamation, "Error loading Koala image file")
            Exit Sub
        End If

        PicW = 160
        PicH = 200
        CharCol = Int(PicW / 4)
        CharRow = Int(PicH / 8)

        TSSL.Text = "Opening Koala file: " + InputFilePath
        Refresh()

        ReDim BMP((CharCol * PicH) - 1)
        ReDim Blank0((CharCol * CharRow) - 1), Blank1((CharCol * CharRow) - 1), Blank2((CharCol * CharRow) - 1)
        ReDim ScrHi((CharCol * CharRow) - 1), ScrLo((CharCol * CharRow) - 1), ColRAM((CharCol * CharRow) - 1), ScrRAM((CharCol * CharRow) - 1)

        For I As Integer = 0 To 999
            ScrHi(I) = Int(kla(8002 + I) / 16)
            ScrLo(I) = kla(8002 + I) Mod 16
            ColRAM(I) = kla(9002 + I) And &HF
        Next

        SHBitmap = New Bitmap(PicW * 2, PicH, Imaging.PixelFormat.Format32bppArgb)
        SLBitmap = New Bitmap(PicW * 2, PicH, Imaging.PixelFormat.Format32bppArgb)
        CRBitmap = New Bitmap(PicW * 2, PicH, Imaging.PixelFormat.Format32bppArgb)
        ACBitmap = New Bitmap(PicW * 2, PicH, Imaging.PixelFormat.Format32bppArgb)

        Dim ColorBG As Color = Color.FromArgb(C64Palettes(BGCol), C64Palettes(BGCol + 16), C64Palettes(BGCol + 32))

        For X As Integer = 0 To (PicW * 2) - 1
            For Y As Integer = 0 To PicH - 1
                ACBitmap.SetPixel(X, Y, ColorBG)
            Next
        Next

        For CY As Integer = 0 To CharRow - 1
            For CX = 0 To CharCol - 1
                Dim CP As Integer = (CY * CharCol) + CX

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

        ReDim BGCols(0)
        BGCols(0) = kla(10002)
        BGCol = kla(10002)

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
                Bits = kla(2 + PI) And BitMask
                Select Case Bits
                    Case 0
                        Col = BGCol
                    Case &H1, &H4, &H10, &H40
                        Col = ScrHi(CI)
                    Case &H2, &H8, &H20, &H80
                        Col = ScrLo(CI)
                    Case &H3, &HC, &H30, &HC0
                        Col = ColRAM(CI)
                End Select

                Dim RGB As Color = Color.FromArgb(C64Palettes(Col), C64Palettes(Col + 16), C64Palettes(Col + 32))

                C64Bitmap.SetPixel(X * 2, Y, RGB)
                C64Bitmap.SetPixel((X * 2) + 1, Y, RGB)
                OrigBitmap.SetPixel(X * 2, Y, RGB)
                OrigBitmap.SetPixel((X * 2) + 1, Y, RGB)
            Next
        Next

        FromKla = True
        C64Formats = FromKla

        PbxPic.Image = C64Bitmap
        PbxSH.Image = SHBitmap
        PbxSL.Image = SLBitmap
        PbxCR.Image = CRBitmap
        PbxAC.Image = ACBitmap

        PbxPic.BringToFront()

        ResizePbx()
        TSSL.Text = "Viewing Koala file: " + InputFilePath
        Exit Sub

Err:
        MsgBox(ErrorToString(), vbOKOnly + vbExclamation, Reflection.MethodBase.GetCurrentMethod.Name + " Error")
        Cursor = Cursors.Default

    End Sub

    Private Sub FrmSPOT_KeyDown(sender As Object, e As KeyEventArgs) Handles Me.KeyDown
        On Error GoTo Err

        Select Case e.KeyCode
            Case Keys.D1, Keys.NumPad1
                PbxCR.BringToFront()
            Case Keys.D2, Keys.NumPad2
                PbxSH.BringToFront()
            Case Keys.D3, Keys.NumPad3
                PbxSL.BringToFront()
            Case Keys.D4, Keys.NumPad4
                PbxAC.BringToFront()
            Case Keys.D5, Keys.NumPad5
                PbxPic.BringToFront()
        End Select

        Exit Sub
Err:
        MsgBox(ErrorToString(), vbOKOnly + vbExclamation, Reflection.MethodBase.GetCurrentMethod.Name + " Error")

    End Sub

    Private Sub FrmSPOT_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        On Error GoTo Err

        With Pnl
            .Left = 0
            .Top = 0
            .Width = Width - 16
            .Height = Height - 100 - STB.Height

            If .AutoScroll = True Then
                ScrollPos.X = Math.Abs(.AutoScrollPosition.X)
                ScrollPos.Y = Math.Abs(.AutoScrollPosition.Y)
                .AutoScroll = False
            Else
                ScrollPos.X = -1
                ScrollPos.Y = -1
            End If
        End With

        With BtnSettings
            .Left = 12
            .Top = Height - 85 - STB.Height
        End With

        With BtnLoadKla
            .Left = Width - (.Width + 20) * 2
            .Top = Height - 85 - STB.Height
        End With

        With BtnOptimize
            .Left = Width - (.Width + 30)
            .Top = Height - 85 - STB.Height
        End With

        If PicW = 0 Then PicW = 160
        If PicH = 0 Then PicH = 200

        With PbxPic
            .Width = (PicW * 4) ' - If((PicW = Int(Pnl.Width / 4)) And (PicH > Int(Pnl.Height / 2)), 18, 0)
            .Height = (PicH * 2) ' - If((PicW > Int(Pnl.Width / 4)) And (PicH = Int(Pnl.Height / 2)), 18, 0)
            .Left = If(.Width > Pnl.Width, 0, Int((Pnl.Width - .Width) / 2))
            .Top = If(.Height > Pnl.Height, 0, Int((Pnl.Height - .Height) / 2))
        End With

        With PbxAC
            .Width = (PicW * 4) ' - If((PicW = Int(Pnl.Width / 4)) And (PicH > Int(Pnl.Height / 2)), 18, 0)
            .Height = (PicH * 2) ' - If((PicW > Int(Pnl.Width / 4)) And (PicH = Int(Pnl.Height / 2)), 18, 0)
            .Left = If(.Width > Pnl.Width, 0, Int((Pnl.Width - .Width) / 2))
            .Top = If(.Height > Pnl.Height, 0, Int((Pnl.Height - .Height) / 2))
        End With

        With PbxSH
            .Width = (PicW * 4) ' - If((PicW = Int(Pnl.Width / 4)) And (PicH > Int(Pnl.Height / 2)), 18, 0)
            .Height = (PicH * 2) ' - If((PicW > Int(Pnl.Width / 4)) And (PicH = Int(Pnl.Height / 2)), 18, 0)
            .Left = If(.Width > Pnl.Width, 0, Int((Pnl.Width - .Width) / 2))
            .Top = If(.Height > Pnl.Height, 0, Int((Pnl.Height - .Height) / 2))
        End With

        With PbxSL
            .Width = (PicW * 4) ' - If((PicW = Int(Pnl.Width / 4)) And (PicH > Int(Pnl.Height / 2)), 18, 0)
            .Height = (PicH * 2) ' - If((PicW > Int(Pnl.Width / 4)) And (PicH = Int(Pnl.Height / 2)), 18, 0)
            .Left = If(.Width > Pnl.Width, 0, Int((Pnl.Width - .Width) / 2))
            .Top = If(.Height > Pnl.Height, 0, Int((Pnl.Height - .Height) / 2))
        End With

        With PbxCR
            .Width = (PicW * 4) ' - If((PicW = Int(Pnl.Width / 4)) And (PicH > Int(Pnl.Height / 2)), 18, 0)
            .Height = (PicH * 2) ' - If((PicW > Int(Pnl.Width / 4)) And (PicH = Int(Pnl.Height / 2)), 18, 0)
            .Left = If(.Width > Pnl.Width, 0, Int((Pnl.Width - .Width) / 2))
            .Top = If(.Height > Pnl.Height, 0, Int((Pnl.Height - .Height) / 2))
            If (.Width > Pnl.Width) Or (.Height > Pnl.Height) Then
                If ScrollPos.X = -1 Then
                    If .Width > Pnl.Width Then
                        ScrollPos.X = Int((.Width - Pnl.Width) / 2)
                    Else
                        ScrollPos.X = 0
                    End If
                End If
                If ScrollPos.Y = -1 Then
                    If .Height > Pnl.Height Then
                        ScrollPos.Y = Int((.Height - Pnl.Height) / 2)
                    Else
                        ScrollPos.Y = 0
                    End If
                End If
                Pnl.AutoScroll = True
                Pnl.AutoScrollPosition = ScrollPos
            End If
        End With

        Exit Sub
Err:
        MsgBox(ErrorToString(), vbOKOnly + vbExclamation, Reflection.MethodBase.GetCurrentMethod.Name + " Error")

    End Sub

End Class
