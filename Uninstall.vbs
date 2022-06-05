Option Explicit
On Error Resume Next

Dim objFSO          ' FileSystemObject
Dim strDelFile      ' 削除するファイル名
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
    response = MsgBox("データベースファイルを削除しますか？", 4, "KMY競馬アンインストール")
End If

If response = vbYes Then
    Set objFSO = WScript.CreateObject("Scripting.FileSystemObject")
    If Err.Number = 0 Then
        objFSO.DeleteFile strDelFile, True
        If Err.Number = 0 Then
            ' MsgBox "データベースを削除しました", 0, "KMY競馬アンインストール"
        Else
            MsgBox "エラー: " & Err.Description, 0, "KMY競馬アンインストール"
            MsgBox "ファイルが正常に削除できませんでした。GitHubの説明書を参照してください"
        End If
    Else
        MsgBox "エラー: " & Err.Description, 0, "KMY競馬アンインストール"
        MsgBox "ファイルが正常に削除できませんでした。GitHubの説明書を参照してください"
    End If
End If
