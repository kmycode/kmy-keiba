﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Data.Db
{
  public class DownloaderTaskData : AppDataBase
  {
    public DownloaderCommand Command { get; set; }

    public string Parameter { get; set; } = string.Empty;

    public bool IsFinished { get; set; }

    public bool IsCanceled { get; set; }

    public bool IsStarted { get; set; }

    public int ProcessId { get; set; }

    public DownloaderError Error { get; set; }

    public string Result { get; set; } = string.Empty;
  }

  public enum DownloaderCommand : short
  {
    Unknown = 0,

    [DownloaderCommand("initialization")]
    Initialization = 1,

    [DownloaderCommand("download")]
    DownloadSetup = 2,

    [DownloaderCommand("shutdown")]
    [Obsolete("ファイルを使ったやり取りに変更。DownloaderConnectorクラスを参照")]
    Shutdown = 3,

    [DownloaderCommand("jvconfig")]
    OpenJvlinkConfigs = 4,

    [DownloaderCommand("nvconfig")]
    OpenNvlinkConfigs = 5,

    [DownloaderCommand("dwrt")]
    DownloadRealTimeData = 6,

    [DownloaderCommand("movie")]
    OpenMovie = 7,

    [DownloaderCommand("movielist")]
    OpenMovieList = 8,

    [DownloaderCommand("rthost")]
    StartRealTimeHost = 9,

    [DownloaderCommand("killhost")]
    KillRealTimeHost = 10,

    [DownloaderCommand("unlha")]
    Unlha = 11,

    [DownloaderCommand("checkprocess")]
    CheckProcessId = 12,
  }

  public enum DownloaderError : short
  {
    Succeed = 0,

    [DownloaderError("不明なエラーが発生しました")]
    Undefined = 1,

    [DownloaderError("ダウンローダの内部エラー")]
    ApplicationError = 2,

    [DownloaderError("アプリとダウンローダのバージョンが異なります")]
    InvalidVersion = 3,

    [DownloaderError("ダウンローダが起動できませんでした")]
    ProcessNotStarted = 4,

    [DownloaderError("ダウンローダとの接続がタイムアウトしました")]
    ConnectionTimeout = 5,

    [DownloaderError("ダウンローダの内部エラー")]
    ApplicationRuntimeError = 6,

    [DownloaderError("必要なソフトウェアがインストールされていません")]
    NotInstalledCom = 7,

    [DownloaderError("処理がタイムアウトしました")]
    Timeout = 8,

    [DownloaderError("セットアップダイアログをキャンセルしました")]
    SetupDialogCanceled = 9,

    [DownloaderError("ライセンスキーが設定されていません")]
    LicenceKeyNotSet = 10,

    [DownloaderError("ライセンスキーの有効期限が切れています")]
    LicenceKeyExpired = 11,

    [DownloaderError("サーバーメンテナンス中です")]
    InMaintance = 12,

    [DownloaderError("操作はキャンセルされました")]
    Canceled = 13,

    [DownloaderError("データベースとの接続がタイムアウトしました")]
    DatabaseTimeout = 14,

    [DownloaderError("ダウンロード対象が存在しません")]
    TargetsNotExists = 15,

    [DownloaderError("サーバーエラー。インターネット未接続かメンテナンスの可能性があります")]
    ServerError = 16,

    [DownloaderError("認証で問題が発生しました")]
    AuthenticationError = 17,

    [DownloaderError("レーシングビューアー連携機能が有効ではありません")]
    RacingViewerNotAvailable = 18,

    [DownloaderError("JV-Linkの動作が確認できません。管理者権限でKMY競馬を再起動してください")]
    NotRunningJVLinkAgent = 19,

    [DownloaderError("ダウンローダが異常停止しました")]
    NotRunningDownloader = 20,
  }

  internal class DownloaderCommandAttribute : Attribute
  {
    public string CommandText { get; }

    public DownloaderCommandAttribute(string text)
    {
      this.CommandText = text;
    }
  }

  internal class DownloaderErrorAttribute : Attribute
  {
    public string Message { get; }

    public DownloaderErrorAttribute(string text)
    {
      this.Message = text;
    }
  }

  public static class DownloaderDataExtensions
  {
    public static string GetCommandText(this DownloaderCommand cmd)
    {
      var attribute = typeof(DownloaderCommand).GetField(cmd.ToString())?.GetCustomAttributes(true).OfType<DownloaderCommandAttribute>();
      return attribute?.FirstOrDefault()?.CommandText ?? string.Empty;
    }
    public static string GetErrorText(this DownloaderError cmd)
    {
      var attribute = typeof(DownloaderError).GetField(cmd.ToString())?.GetCustomAttributes(true).OfType<DownloaderErrorAttribute>();
      return attribute?.FirstOrDefault()?.Message ?? string.Empty;
    }
  }
}
