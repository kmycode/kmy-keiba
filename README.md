# KMY競馬

Windowsで動作する競馬情報閲覧用のアプリです。JRA-VAN、地方競馬DATAのデータを使用します。  
アプリの説明書、利用方法についてはWikiを参照してください。

<img src="https://github.com/kmycode/kmy-keiba/blob/develop/Assets/main.png" width="500"/>

## プロジェクト

| プロジェクト名 | 説明 |
| --- | --- |
| `KmyKeiba` | メインアプリ。64bitでのビルドを想定しています |
| `KmyKeiba.Downloader` | 競馬データをダウンロードするアプリ。<br>現状はハードコーディングです。プログラムコードを書き換えながら使ってます。<br>後述する特殊な場合を除き、32bitでビルドしてください |
| `KmyKeiba.JVLink` | JV-LINK、地方競馬DATAよりデータをダウンロードするためのプログラムです |
| `KmyKeiba.Data` | 競馬データの型定義が含まれます。ここにある`MyContextBase`を継承したクラスを利用することで、データベースへアクセスできます |
| それ以外 | 今はメンテナンスしていません。`Keras.NET`を使用したディープラーニングとかとかのコードが入ってます。欲しい人は適当に持ってってください。サポートはしません |

## 動作に必要なもの

[初期設定](https://github.com/kmycode/kmy-keiba/wiki/%E5%88%9D%E6%9C%9F%E8%A8%AD%E5%AE%9A)をご覧ください。

## ビルド

.NET 8.0とC# 12で開発しているため、Visual Studio 2022以降が必要です。VSインストール時に、デスクトップアプリにチェックを入れてください。

### ビルドする場合の制約事項

GitHubにて公開しているソースコードには、以下の機能が含まれません。

- KSC馬券購入プラグインとの連携ロジック

ソースコードを下記の手順に従ってビルドすると、馬券購入機能はご利用になれません。これらは別途Releaseより配布するバイナリには含まれています。あらかじめご了承ください。Wikiのライセンスもお読みになってください。

### ビルドに必要なもの

アプリの実行では片方だけで構わないのですが、ビルドする場合は、上記「JV-Link」「UmaConn」の両方が必要です。

また、このリポジトリでは、著作権の関係で欠落しているファイルが存在しており、それがないとビルドできません。  
[Data Lab. SDK](https://jra-van.jp/dlb/sdv/sdk.html)よりSDK本体をダウンロードしてください。Ver.4.6.0では、以下のような構成になっています。

```
JV-Data構造体
JV-Link
サンプルプログラム
ドキュメント
```

このうち「JV-Data構造体」フォルダの中の「C#版」に含まれる `JVData_Struct.cs` ファイルを `structures.cs` にリネームのうえ、以下のディレクトリにコピーしてください。  
`JVLib` フォルダがない場合は作成してください。

```
KmyKeiba.JVLink/JVLib/structures.cs
```

さらに、 `structures.cs` を以下のように編集してください。

```c#
using System.Text;

#nullable disable

namespace KmyKeiba.JVLink.Wrappers.JVLib
{

    // <ここに元々のstructures.csの内容を挿入>

    // <JVData_Structの最初の行にpartialを追加してください>
    // public static partial class JVData_Struct

}
```

### ビルド手順

以下の順序でビルドしてください。

```
KmyKeiba.Downloader (x86) -> KmyKeiba (x64)
```

そのあと、`KmyKeiba (x64)`を実行します。

### `Add-Migration` について

このプログラムはEntityFrameworkCoreを使用しています。`Add-Migration` を実行するときには、以下の手順が必要です。

1. プロジェクトのソリューションプラットフォームを`x64`にします
1. パッケージマネージャーコンソール、スタートアッププロジェクト、いずれも`KmyKeiba.Downloader`に設定します
1. `Add-Migration`を実行します
1. プロジェクトのソリューションプラットフォームを`x86`にします
1. `KmyKeiba.Downloader`をビルドします
1. プロジェクトのソリューションプラットフォームを`x64`にします
1. `KmyKeiba`をビルドします
