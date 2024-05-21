using KmyKeiba.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Connection
{

  [Flags]
  public enum DownloadLink
  {
    None = 0,

    [Label("中央")]
    Central = 0b01,

    [Label("地方")]
    Local = 0b10,

    [Label("JRDB")]
    Jrdb = 0b100,
  }

  enum DownloadingType
  {
    [Label("レース")]
    Race,

    [Label("オッズ")]
    Odds,

    [Label("JRDB")]
    Jrdb,
  }

  enum LoadingProcessValue
  {
    Unknown,

    [Label("接続オープン中")]
    Opening,

    [Label("ダウンロード中")]
    Downloading,

    [Label("データ読み込み中")]
    Loading,

    [Label("データ保存中")]
    Writing,

    [Label("後処理中")]
    Processing,

    [Label("接続クローズ中")]
    Closing,

    [Label("JRA-VANからのお知らせが表示されています")]
    CheckingJraVanNews,
  }

  enum DownloadMode
  {
    Continuous,
    WithStartDate,
  }

  enum DownloadingDataspec
  {
    Unknown,

    [Label("レース結果")]
    RB12,

    [Label("レース情報")]
    RB15,

    [Label("タイム型MING")]
    RB13,

    [Label("対戦型MING")]
    RB17,

    [Label("オッズ")]
    RB30,

    [Label("馬体重")]
    RB11,

    [Label("変更情報")]
    RB14,

    [Label("時系列オッズ")]
    RB41,
  }

  [Flags]
  public enum ProcessingStep
  {
    Unknown = 0,

    [Label("不正なデータを処理中")]
    InvalidData = 1,

    [Label("脚質を計算中")]
    RunningStyle = 2,

    [Label("基準タイムを計算中")]
    StandardTime = 4,

    [Label("馬データの成型中")]
    PreviousRaceDays = 8,

    [Label("騎手の勝率を計算中")]
    RiderWinRates = 16,

    [Label("地方競馬のレース条件を解析中")]
    RaceSubjectInfos = 32,

    [Label("データをマイグレーション中 (from 2.5.0)")]
    MigrationFrom250 = 64,

    [Label("データをマイグレーション中 (from 3.2.2)")]
    MigrationFrom322 = 128,

    [Label("拡張情報を作成中")]
    HorseExtraData = 256,

    [Label("拡張情報をリセット中")]
    ResetHorseExtraData = 512,

    [Label("データをマイグレーション中 (from 4.3.0)")]
    MigrationFrom430 = 1024,

    [Label("データをマイグレーション中 (from 5.0.0)")]
    MigrationFrom500 = 2048,

    [Label("複勝オッズをコピー中")]
    CopyPlaceOdds = 4096,
  }
}
