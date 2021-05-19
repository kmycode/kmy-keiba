using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.JVLink.Wrappers
{
  public class JVLinkException<T> : Exception where T : System.Enum
  {
    public T Code { get; init; }

    public JVLinkException(T code) : base()
    {
      this.Code = code;
    }

    public JVLinkException(T code, Exception inner) : base(string.Empty, inner)
    {
      this.Code = code;
    }
  }

  public enum JVLinkCommonCode
  {
    [JVLinkCode("不明")]
    Unknown,
  }

  public enum JVLinkInitializeResult
  {
    [JVLinkCode("不明")]
    Unknown,
  }

  public enum JVLinkLoadResult
  {
    [JVLinkCode("これ以上データはありません")]
    Exit = -1,

    [JVLinkCode("セットアップダイアログでキャンセルが押されました")]
    SetupCanceled = -2,

    [JVLinkCode("存在しないデータです")]
    InvalidDataspec = -111,

    [JVLinkCode("開始日時が誤っています")]
    InvalidFromTime = -112,

    [JVLinkCode("終了日時が誤っています")]
    InvalidToTime = -113,

    [JVLinkCode("日時が誤っています")]
    InvalidKey = -114,

    [JVLinkCode("取得するデータ種別が誤っています")]
    InvalidOption = -115,

    [JVLinkCode("指定されたデータはこのオプションでは取得できません")]
    InvalidDatespecAndOption = -116,

    [JVLinkCode("初期化が行われていません")]
    NoInitialized = -201,

    [JVLinkCode("すでに接続が開かれています")]
    AlreadyOpen = -202,

    [JVLinkCode("レジストリの値が不正です")]
    InvalidRegistry = -211,

    [JVLinkCode("認証エラーです")]
    AuthenticationError = -301,

    [JVLinkCode("利用キーが不正です")]
    LicenceKeyExpired = -302,

    [JVLinkCode("利用キーが設定されていません")]
    LicenceKeyNotSet = -303,

    [JVLinkCode("内部エラー")]
    InternalError = -401,

    [JVLinkCode("サーバーエラー404")]
    NotFound = -411,

    [JVLinkCode("サーバーエラー403")]
    Forbidden = -412,

    [JVLinkCode("サーバーエラー")]
    ServerError = -413,

    [JVLinkCode("サーバーの不正な応答")]
    InvalidServerResponse = -421,

    [JVLinkCode("サーバーアプリケーションの不正な応答")]
    InvalidServerApplication = -431,

    [JVLinkCode("スタートキット（CD-ROM）が無効です")]
    InvalidStartKit = -501,

    [JVLinkCode("現在サーバーはメンテナンス中です")]
    InMaintance = -504,
  }

  public enum JVLinkReadResult
  {
    [JVLinkCode("新しいファイルへ切り替わりました")]
    NewFile = -1,

    [JVLinkCode("ファイルのダウンロード途中です")]
    Downloading = -3,

    [JVLinkCode("初期化が行われていません")]
    NoInitialized = -201,

    [JVLinkCode("すでに接続が開かれています")]
    AlreadyOpen = -202,

    [JVLinkCode("オープンが行われていません")]
    NotOpened = -203,

    [JVLinkCode("ダウンロードしたファイルサイズが異常です")]
    InvalidDownloadedFileSize = -402,

    [JVLinkCode("ダウンロードしたデータが異常です")]
    InvalidDownloadedData = -403,

    [JVLinkCode("ダウンロードに失敗しました")]
    DownloadError = -502,

    [JVLinkCode("ダウンロードしたファイルが見つかりません")]
    DownloadedFileNotFound = -503,
  }

  public class JVLinkException : JVLinkException<JVLinkCommonCode>
  {
    public static JVLinkException<T> GetError<T>(T code) where T : System.Enum => new JVLinkException<T>(code);
    public static JVLinkException<T> GetError<T>(T code, Exception inner) where T : System.Enum => new JVLinkException<T>(code, inner);

    public static JVLinkCodeAttribute GetAttribute(object code)
    {
      var type = code.GetType();
      var fieldInfo = type.GetField(code.ToString()!);
      if (fieldInfo == null)
      {
        return new JVLinkCodeAttribute("不明");
      }
      var attributes = fieldInfo.GetCustomAttributes(typeof(JVLinkCodeAttribute), false) as JVLinkCodeAttribute[];
      if (attributes != null && attributes.Length > 0)
      {
        return attributes[0];
      }
      return new JVLinkCodeAttribute("不明");
    }

    public JVLinkException() : base(JVLinkCommonCode.Unknown)
    {
    }

    public JVLinkException(Exception inner) : base(JVLinkCommonCode.Unknown, inner)
    {
    }
  }
}
