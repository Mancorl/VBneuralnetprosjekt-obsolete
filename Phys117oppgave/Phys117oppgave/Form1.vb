Public Class Form1
    Public Property Label1 As Object

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim FileNum As Integer = FreeFile()
        Dim TempS As String = ""
        Dim TempL As String
        FileOpen(FileNum, "error.txt", OpenMode.Input)
        Do Until EOF(FileNum)
            TempL = LineInput(FileNum)
            TempS += TempL + vbCrLf
        Loop
        FileClose(FileNum)
        Label1.Text = TempS
    End Sub
End Class
