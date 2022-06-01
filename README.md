# KMY競馬

Windowsで動作する競馬情報閲覧用のアプリです。JRA-VAN、地方競馬DATAのデータを使用します。

<img src="https://github.com/kmycode/kmy-keiba/blob/develop/Assets/main.png" width="400"/>

## プロジェクト

| プロジェクト名 | 説明 |
| --- | --- |
| `KmyKeiba` | メインアプリ。64bitでのビルドを想定しています |
| `KmyKeiba.Downloader` | 競馬データをダウンロードするアプリ。<br>現状はハードコーディングです。プログラムコードを書き換えながら使ってます。<br>後述する特殊な場合を除き、32bitでビルドしてください |
| `KmyKeiba.JVLink` | JV-LINK、地方競馬DATAよりデータをダウンロードするためのプログラムです |
| `KmyKeiba.Data` | 競馬データの型定義が含まれます。ここにある`MyContextBase`を継承したクラスを利用することで、データベースへアクセスできます |
| それ以外 | 今はメンテナンスしていません。`Keras.NET`を使用したディープラーニングとかとかのコードが入ってます。欲しい人は適当に持ってってください。サポートはしません |

## 動作に必要なもの

現在バイナリは配布していないので、ビルドしないと起動できない状態です。（後日Releaseより配布予定です）  
アプリそのものは無償ですが、競馬データを入手するには以下の利用キーが必須です。

### 利用キー（有償）およびデータ取得のためのソフト

競馬データの取得には、別途有償契約が必要です。（中央＋地方の場合、毎月約4000円）  
下記の片方または両方を契約しないと、競馬データが取得できず、事実上本アプリをご利用いただけません。

アプリは、必要なものだけをインストールいただいて差し支えありません。

* [JRA-VANデータラボ会員](https://jra-van.jp/dlb/)
  * 対応アプリ：[JV-Link](https://jra-van.jp/dlb/) - 「動作環境（JV-Link）」タブよりダウンロードできます
  * 中央競馬のデータ取得に必要です
  * 契約後、利用キーを取得してください。「JRAレーシングビュアー」は今のところ不要です
* [地方競馬DATA](https://saikyo.k-ba.com/members/chihou/)
  * 対応アプリ：[UmaConn](https://saikyo.k-ba.com/members/chihou/) - ダウンロードボタンよりダウンロードできます
  * 地方競馬のデータ取得に必要です
  * 契約後、利用キーを取得してください。利用キー以外にも多数の購入オプションがありますが全て不要です

利用キーはデータベースインストール時に設定が可能です。

## ビルド

.NET 6.0とC# 10で開発しているため、Visual Studio 2022以降が必要です。VSインストール時に、デスクトップアプリにチェックを入れてください。

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

### `Add-Migration` について

このプログラムはEntityFrameworkCoreを使用しています。`Add-Migration` を実行するときには、以下の手順が必要です。

- プロジェクトのソリューションプラットフォームを`x64`にしてください
- パッケージマネージャーコンソール、スタートアッププロジェクト、いずれも`KmyKeiba.Downloader`に設定してください
- `Add-Migration`の前に、`KmyKeiba`（アプリ本体）を**リビルド**してください
