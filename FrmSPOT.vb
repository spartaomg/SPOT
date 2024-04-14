Imports System.IO

Public Class FrmSPOT
    Private Sub Main_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        On Error GoTo Err

        Dim Splash As New FrmSplash
        Dim FInfo, OutInfo As FileInfo
        CmdArg = Environment.GetCommandLineArgs()

        If CmdArg.Count > 1 Then

            CmdLn = True

            Dim i As Integer = 1

            While i < CmdArg.Count

                If i = 1 Then
                    InputFilePath = CmdArg(i)

                    If LCase(InputFilePath) = "-h" Then
                        ShowHelp()
                        Close()
                        Exit Sub
                    End If
                ElseIf (CmdArg(i) = "-o") Or (CmdArg(i) = "-O") Then
                    If i + 1 < CmdArg.Count Then
                        i += 1
                        CmdOut = CmdArg(i)
                    Else
                        MsgBox("***CRITICAL***" + vbNewLine + "Missing [output] parameter.", vbCritical + vbOKOnly)
                        Close()
                        Exit Sub
                    End If
                ElseIf (CmdArg(i) = "-f") Or (CmdArg(i) = "-F") Then        'output type(s)
                    If i + 1 < CmdArg.Count Then
                        i += 1
                        CmdOptions = CmdArg(i)
                    Else
                        MsgBox("***CRITICAL***" + vbNewLine + "Missing [format] parameter.", vbCritical + vbOKOnly)
                        Close()
                        Exit Sub
                    End If
                ElseIf (CmdArg(i) = "-b") Or (CmdArg(i) = "-BF") Then        'output type(s)
                    If i + 1 < CmdArg.Count Then
                        i += 1
                        CmdColors = LCase(CmdArg(i))
                    Else
                        MsgBox("***CRITICAL***" + vbNewLine + "Missing [background color] parameter.", vbCritical + vbOKOnly)
                        Close()
                        Exit Sub
                    End If
                Else
                    MsgBox("***CRITICAL***" + vbNewLine + "Unrecognized option: " + CmdArg(i), vbCritical + vbOKOnly)
                    Close()
                    Exit Sub
                End If
                i += 1
            End While

            If CmdColors = "" Then
                CmdColors = "0123456789abcdef"                      'If argument is not specified, we allow any background colors
            End If

            Splash.Show()

            'InputFilePath = LCase(CmdArg(1))                        'Input file path and name

            'CmdOptions = ""
            'CmdOut = ""
            'If CmdArg.Length > 2 Then
            'CmdOptions = LCase(CmdArg(2))                       'Command line options
            'End If
            'If CmdArg.Length > 3 Then
            'CmdOut = LCase(CmdArg(3))
            'End If
            'If CmdArg.Length > 4 Then
            'CmdColors = LCase(CmdArg(4))
            'Else
            'CmdColors = "0123456789abcdef"                      'If argument is not specified, we allow any background colors
            'End If
            If CmdOptions = "" Then
                With My.Settings
                    If .OutputKla Then CmdOptions += "k"
                    If .OutputMap Then CmdOptions += "m"
                    If .OutputCol Then CmdOptions += "c"
                    If .OutputScr Then CmdOptions += "s"
                    If .OutputBgc Then CmdOptions += "g"
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

ExitNoErr:  Close()                                 'This will close the main form and set the exit code and exit Sparkle

            Exit Sub                                'This is needed to prevent Error Chime on exit
        End If

        TSSL.Text = "No file selected"

        Text = "SPOT - Sparta's Picture Optimizing Tool for the Commodore 64 GUI version " + My.Application.Info.Version.Major.ToString + "." + My.Application.Info.Version.Minor.ToString +
        "." + My.Application.Info.Version.Build.ToString + "." + If(Len(My.Application.Info.Version.Revision.ToString) = 3, "0", "") +
        My.Application.Info.Version.Revision.ToString ' + ")"

        Exit Sub

Err:
        Splash.Close()
        MsgBox(ErrorToString(), vbOKOnly + vbExclamation, Reflection.MethodBase.GetCurrentMethod.Name + " Error")
        Cursor = Cursors.Default

        If CmdArg.Length > 1 Then Close()

    End Sub

    Private Sub ShowHelp()
        Dim HelpText As String = ""

        HelpText += "SPOT is a small PC tool that converts .png, .bmp, and .kla images into C64 file formats "
        HelpText += "optimized for better compression. This version of SPOT has a GUI but it can be also used as a command-line tool." + vbNewLine + vbNewLine
        HelpText += "Command-line usage" + vbNewLine
        HelpText += "---------------------------" + vbNewLine + vbNewLine
        HelpText += "spot input -o [output] -f [format] -b [bgcolor]" + vbNewLine + vbNewLine
        HelpText += "input:   An input image file to be optimized/converted. Only .png, .bmp, and .kla file types are accepted." + vbNewLine + vbNewLine
        HelpText += "output:  The output folder and file name. File extension (if exists) will be ignored. If omitted, SPOT will create "
        HelpText += "a <spot/input> folder and the input file's name will be used as output file name." + vbNewLine + vbNewLine
        HelpText += "format:  Output file formats: kmscg2obpj. This parameter is optional. If omitted, then the default Koala file will be created."
        HelpText += "Select as many as you want in any order:" + vbNewLine
        HelpText += "           k - .kla (Koala - 10003 bytes)" + vbNewLine
        HelpText += "           m - .map (bitmap data)" + vbNewLine
        HelpText += "           s - .scr (screen RAM data)" + vbNewLine
        HelpText += "           c - .col (color RAM data)" + vbNewLine
        HelpText += "           g - .bgc (background color)" + vbNewLine
        HelpText += "           2 - .ccr (compressed color RAM data): two adjacent half bytes are combined to reduce the size of the color RAM to 500 bytes." + vbNewLine
        HelpText += "           o - .obm (optimized bitmap - 9503 bytes): bitmap data is stored column wise. Screen RAM and compressed color RAM are stored "
        HelpText += "row wise. First two bytes are address bytes ($00, $60) and the last one is " + vbNewLine

        HelpText += "                   the background color as in the Koala format. File size:  9503 bytes. In most cases, this format compresses somewhat better than Koala but it also needs a more "
        HelpText += "complex display routine." + vbNewLine
        HelpText += "           b - .bmp" + vbNewLine
        HelpText += "           p - .png" + vbNewLine
        HelpText += "           j - .jpg" + vbNewLine + vbNewLine
        HelpText += "bgcolor: Output background color(s): 0123456789abcdef or x. SPOT will only create C64 files using the selected "
        HelpText += "background color(s). If x is used as value then only the first possible background color will be used," + vbNewLine
        HelpText += "           all other possible background colors will be ignored. If this option is omitted, then SPOT will generate "
        HelpText += "output files using all possible background colors. If more than one background color is possible (and " + vbNewLine
        HelpText += "           allowed) then SPOT will append the background color to the output file name." + vbNewLine + vbNewLine
        HelpText += "Examples" + vbNewLine
        HelpText += "------------" + vbNewLine + vbNewLine
        HelpText += "spot picture.bmp -o newfolder/newfile -f msc -b 0" + vbNewLine
        HelpText += "SPOT will convert <picture.bmp> to .map, .scr, and .col formats with black as background color and will save them to "
        HelpText += "the <newfolder> folder using <newfile> as output base filename." + vbNewLine + vbNewLine
        HelpText += "spot picture.png -o newfolder/newfile -f msc" + vbNewLine
        HelpText += "SPOT will convert <picture.png> to .map, .scr, and .col formats with all possible background colors and will save them "
        HelpText += "to the <newfolder> folder using <newfile> as output base filename." + vbNewLine + vbNewLine
        HelpText += "spot picture.png -o newfolder/newfile" + vbNewLine
        HelpText += "SPOT will convert <picture.png> to the default Koala format with all possible background colors and will save the "
        HelpText += "output to the <newfolder> folder using <newfile> as output base filename." + vbNewLine + vbNewLine
        HelpText += "spot picture.png" + vbNewLine
        HelpText += "SPOT will convert <picture.png> to the default Koala format with all possible background colors and will save the "
        HelpText += "output to the <spot/picture> folder using <picture> as output base filename" + vbNewLine + vbNewLine
        HelpText += "Notes" + vbNewLine
        HelpText += "-------" + vbNewLine + vbNewLine
        HelpText += "SPOT recognizes several C64 palettes. If a direct palette match is not found then it will convert colors to "
        HelpText += "a standard C64 palette using a lowest-cost algorithm." + vbNewLine + vbNewLine
        HelpText += "SPOT can handle non-standard image sizes (such as the vertical bitmap in Memento Mori and the diagonal bitmap "
        HelpText += "in Christmas Megademo). When a .kla or .obm file is created from a non-standard sized image, SPOT takes a centered "
        HelpText += """snapshot"" of the image and saves that as .kla or .obm. Map, screen RAM, and color RAM files can be of any size." + vbNewLine + vbNewLine
        HelpText += "SPOT is meant to convert and optimize multicolor bitmaps (hi-res images get converted to multicolor)." + vbNewLine + vbNewLine

        MsgBox(HelpText, vbInformation + vbOKOnly, "SPOT help")

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

        ReDim BGCols(0)
        BGCols(0) = kla(10002)
        BGCol = BGCols(0)

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

        With BtnOptimize
            .Left = Width - (.Width + 30)
            .Top = Height - 85 - STB.Height
        End With

        With BtnLoadKla
            .Left = BtnOptimize.Left - (.Width + 6)
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
