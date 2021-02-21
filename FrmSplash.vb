Public Class FrmSplash
    Private Sub FrmSplash_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        On Error GoTo Err

        Width = 64
        Height = 64

        TopMost = True
        Refresh()

        Exit Sub
Err:
        MsgBox(ErrorToString(), vbOKOnly + vbExclamation, Reflection.MethodBase.GetCurrentMethod.Name + " Error")
    End Sub

End Class