Option Explicit
On Error Resume Next

Dim objFSO          ' FileSystemObject
Dim strDelFile      ' �폜����t�@�C����
Dim response
Dim strLocalAppData
Dim oWshShell
Dim FS
Dim ret

Set oWshShell = CreateObject("WScript.Shell")
Set FS = CreateObject("Scripting.FileSystemObject")
strLocalAppData = oWshShell.ExpandEnvironmentStrings("%LOCALAPPDATA%")
strDelFile = strLocalAppData & "\KMYsofts\KMYkeiba\maindata.sqlite3"

ret = FS.FileExists(strDelFile)
If ret = True Then
    response = MsgBox("�f�[�^�x�[�X�t�@�C�����폜���܂����H", 4, "KMY���n�A���C���X�g�[��")
End If

If response = vbYes Then
    Set objFSO = WScript.CreateObject("Scripting.FileSystemObject")
    If Err.Number = 0 Then
        objFSO.DeleteFile strDelFile, True
        If Err.Number = 0 Then
            ' MsgBox "�f�[�^�x�[�X���폜���܂���", 0, "KMY���n�A���C���X�g�[��"
        Else
            MsgBox "�G���[: " & Err.Description, 0, "KMY���n�A���C���X�g�[��"
            MsgBox "�t�@�C��������ɍ폜�ł��܂���ł����BGitHub�̐��������Q�Ƃ��Ă�������"
        End If
    Else
        MsgBox "�G���[: " & Err.Description, 0, "KMY���n�A���C���X�g�[��"
        MsgBox "�t�@�C��������ɍ폜�ł��܂���ł����BGitHub�̐��������Q�Ƃ��Ă�������"
    End If
End If
