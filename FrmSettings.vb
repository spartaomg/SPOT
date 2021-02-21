Imports System.ComponentModel

Public Class FrmSettings
    Private Sub FrmSettings_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        On Error GoTo Err

        LoadSettings()

        Exit Sub
Err:
        MsgBox(ErrorToString(), vbOKOnly + vbExclamation, Reflection.MethodBase.GetCurrentMethod.Name + " Error")

    End Sub

    Private Sub FrmSettings_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        On Error GoTo Err

        SaveSettings()

        Exit Sub
Err:
        MsgBox(ErrorToString(), vbOKOnly + vbExclamation, Reflection.MethodBase.GetCurrentMethod.Name + " Error")

    End Sub

    Private Sub LoadSettings()
        On Error GoTo Err

        ChkKla.Checked = My.Settings.OutputKla
        ChkMap.Checked = My.Settings.OutputMap
        ChkCol.Checked = My.Settings.OutputCol
        ChkScr.Checked = My.Settings.OutputScr
        ChkC2.Checked = My.Settings.OutputCcr
        ChkRmp.Checked = My.Settings.OutputObm
        ChkPng.Checked = My.Settings.OutputPng
        ChkBmp.Checked = My.Settings.OutputBmp
        ChkJpg.Checked = My.Settings.OutputJpg
        ChkAutoSave.Checked = My.Settings.Autosave

        Exit Sub
Err:
        MsgBox(ErrorToString(), vbOKOnly + vbExclamation, Reflection.MethodBase.GetCurrentMethod.Name + " Error")

    End Sub

    Private Sub SaveSettings()
        On Error GoTo Err

        My.Settings.OutputKla = ChkKla.Checked
        My.Settings.OutputMap = ChkMap.Checked
        My.Settings.OutputCol = ChkCol.Checked
        My.Settings.OutputScr = ChkScr.Checked
        My.Settings.OutputCcr = ChkC2.Checked
        My.Settings.OutputObm = ChkRmp.Checked
        My.Settings.OutputPng = ChkPng.Checked
        My.Settings.OutputBmp = ChkBmp.Checked
        My.Settings.OutputJpg = ChkJpg.Checked
        My.Settings.Autosave = ChkAutoSave.Checked

        My.Settings.Save()

        Exit Sub
Err:
        MsgBox(ErrorToString(), vbOKOnly + vbExclamation, Reflection.MethodBase.GetCurrentMethod.Name + " Error")

    End Sub

    Private Sub BtnClose_Click(sender As Object, e As EventArgs) Handles BtnClose.Click
        On Error GoTo Err

        Close()

        Exit Sub
Err:
        MsgBox(ErrorToString(), vbOKOnly + vbExclamation, Reflection.MethodBase.GetCurrentMethod.Name + " Error")

    End Sub
End Class