using System.Text;

#nullable disable

namespace KmyKeiba.JVLink.Wrappers.JVLib
{

  /// <summary>
  /// JRA-VAN Data Lab. JV-Data構造体

  /// </summary>
  /// <remarks>
  /// 最終更新日：2009年12月8日
  /// (C) Copyright JRA SYSTEM SERVICE CO.,LTD. 2009 All rights reserved
  /// </remarks>
  public static partial class JVData_Struct
  {
    #region セットデータのプログラミングパーツ


    /// <summary>
    /// 文字列をバイト長で切出し

    /// </summary>
    /// <param name="myByte">文字列</param>
    /// <param name="bSt">開始位置</param>
    /// <param name="bLen">バイト長</param>
    /// <returns>文字列</returns>
    public static string MidB2S(ref byte[] myByte, int bSt, int bLen)
    {
      // 文字を任意に切出す

      return Encoding.GetEncoding("Shift_JIS").GetString(myByte, bSt - 1, bLen);
    }

    /// <summary>
    /// バイト配列をバイト長で切出し

    /// </summary>
    /// <param name="myByte">文字列</param>
    /// <param name="bSt">開始位置</param>
    /// <param name="bLen">バイト長</param>
    /// <returns>文字列</returns>
    public static byte[] MidB2B(ref byte[] myByte, int bSt, int bLen)
    {
      byte[] cBt = new byte[bLen];
      int j = 0;

      // 文字列バイト任意切り出し

      for (int i = bSt - 1; i < bSt - 1 + bLen; i++)
      {
        cBt[j] = myByte[i];
        j++;
      }

      return cBt;
    }

    /// <summary>
    /// 文字列をバイト配列に変換
    /// </summary>
    /// <param name="myString">文字列</param>
    /// <returns>バイト配列</returns>
    public static byte[] Str2Byte(ref string myString)
    {
      // Shift JISに変換する
      return Encoding.GetEncoding("Shift_JIS").GetBytes(myString);
    }

    #endregion

    #region 共通構造体


    /// <summary>
    /// 年月日
    /// </summary>
    public struct YMD
    {
      public string Year;     // 年
      public string Month;    // 月

      public string Day;      // 日

      // データセット
      public void SetDataB(byte[] bBuff)
      {
        Year = MidB2S(ref bBuff, 1, 4);
        Month = MidB2S(ref bBuff, 5, 2);
        Day = MidB2S(ref bBuff, 7, 2);
      }
    }

    /// <summary>
    /// 時分秒

    /// </summary>
    public struct HMS
    {
      public string Hour;     // 時

      public string Minute;   // 分

      public string Second;   // 秒


      // データセット
      public void SetDataB(byte[] bBuff)
      {
        Hour = MidB2S(ref bBuff, 1, 2);
        Minute = MidB2S(ref bBuff, 3, 2);
        Second = MidB2S(ref bBuff, 5, 2);
      }
    }

    /// <summary>
    /// 時分
    /// </summary>
    public struct HM
    {
      public string Hour;     // 時

      public string Minute;   // 分


      // データセット
      public void SetDataB(byte[] bBuff)
      {
        Hour = MidB2S(ref bBuff, 1, 2);
        Minute = MidB2S(ref bBuff, 3, 2);
      }
    }

    /// <summary>
    /// 月日時分
    /// </summary>
    public struct MDHM
    {
      public string Month;    // 月

      public string Day;      // 日
      public string Hour;     // 時

      public string Minute;   // 分


      // データセット
      public void SetDataB(byte[] bBuff)
      {
        Month = MidB2S(ref bBuff, 1, 2);
        Day = MidB2S(ref bBuff, 3, 2);
        Hour = MidB2S(ref bBuff, 5, 2);
        Minute = MidB2S(ref bBuff, 7, 2);
      }
    }

    /// <summary>
    /// レコードヘッダ
    /// </summary>
    public struct RECORD_ID
    {
      public string RecordSpec;   // レコード種別
      public string DataKubun;    // データ区分

      public YMD MakeDate;        // データ作成年月日

      // データセット
      public void SetDataB(byte[] bBuff)
      {
        RecordSpec = MidB2S(ref bBuff, 1, 2);
        DataKubun = MidB2S(ref bBuff, 3, 1);
        MakeDate.SetDataB(MidB2B(ref bBuff, 4, 8));
      }
    }

    /// <summary>
    /// 競走識別情報
    /// </summary>
    public struct RACE_ID
    {
      public string Year;         // 開催年
      public string MonthDay;     // 開催月日
      public string JyoCD;        // 競馬場コード

      public string Kaiji;        // 開催回[第N回]
      public string Nichiji;      // 開催日目[N日目]
      public string RaceNum;      // レース番号

      // データセット
      public void SetDataB(byte[] bBuff)
      {
        Year = MidB2S(ref bBuff, 1, 4);
        MonthDay = MidB2S(ref bBuff, 5, 4);
        JyoCD = MidB2S(ref bBuff, 9, 2);
        Kaiji = MidB2S(ref bBuff, 11, 2);
        Nichiji = MidB2S(ref bBuff, 13, 2);
        RaceNum = MidB2S(ref bBuff, 15, 2);
      }
    }

    /// <summary>
    /// 競走識別情報2
    /// </summary>
    public struct RACE_ID2
    {
      public string Year;         // 開催年
      public string MonthDay;     // 開催月日
      public string JyoCD;        // 競馬場コード

      public string Kaiji;        // 開催回[第N回]
      public string Nichiji;      // 開催日目[N日目]

      // データセット
      public void SetDataB(byte[] bBuff)
      {
        Year = MidB2S(ref bBuff, 1, 4);
        MonthDay = MidB2S(ref bBuff, 5, 4);
        JyoCD = MidB2S(ref bBuff, 9, 2);
        Kaiji = MidB2S(ref bBuff, 11, 2);
        Nichiji = MidB2S(ref bBuff, 13, 2);
      }
    }

    /// <summary>
    /// 本年・累計成績情報
    /// </summary>
    public struct SEI_RUIKEI_INFO
    {
      public string SetYear;          // 設定年
      public string HonSyokinTotal;   // 本賞金合計

      public string FukaSyokin;       // 付加賞金合計

      public string[] ChakuKaisu;     // 着回数

      // 配列の初期化

      public void Initialize()
      {
        ChakuKaisu = new string[6];
      }

      // データセット
      public void SetDataB(byte[] bBuff)
      {
        Initialize();
        SetYear = MidB2S(ref bBuff, 1, 4);
        HonSyokinTotal = MidB2S(ref bBuff, 5, 10);
        FukaSyokin = MidB2S(ref bBuff, 15, 10);

        for (int i = 0; i < 6; i++)
        {
          ChakuKaisu[i] = MidB2S(ref bBuff, 25 + (6 * i), 6);
        }
      }
    }

    /// <summary>
    /// 最近重賞勝利情報
    /// </summary>
    public struct SAIKIN_JYUSYO_INFO
    {
      public RACE_ID SaikinJyusyoid;  // <年月日場回日R>
      public string Hondai;           // 競走名本題

      public string Ryakusyo10;       // 競走名略称10字

      public string Ryakusyo6;        // 競走名略称6字

      public string Ryakusyo3;        // 競走名略称3字

      public string GradeCD;          // グレードコード

      public string SyussoTosu;       // 出走頭数
      public string KettoNum;         // 血統登録番号
      public string Bamei;            // 馬名


      // データセット
      public void SetDataB(byte[] bBuff)
      {
        SaikinJyusyoid.SetDataB(MidB2B(ref bBuff, 1, 16));
        Hondai = MidB2S(ref bBuff, 17, 60);
        Ryakusyo10 = MidB2S(ref bBuff, 77, 20);
        Ryakusyo6 = MidB2S(ref bBuff, 97, 12);
        Ryakusyo3 = MidB2S(ref bBuff, 109, 6);
        GradeCD = MidB2S(ref bBuff, 115, 1);
        SyussoTosu = MidB2S(ref bBuff, 116, 2);
        KettoNum = MidB2S(ref bBuff, 118, 10);
        Bamei = MidB2S(ref bBuff, 128, 36);
      }
    }

    /// <summary>
    /// 本年・前年・累計成績情報
    /// </summary>
    public struct HON_ZEN_RUIKEISEI_INFO
    {
      public string SetYear;                      // 設定年
      public string HonSyokinHeichi;              // 平地本賞金合計

      public string HonSyokinSyogai;              // 障害本賞金合計

      public string FukaSyokinHeichi;             // 平地付加賞金合計

      public string FukaSyokinSyogai;             // 障害付加賞金合計

      public CHAKUKAISU6_INFO ChakuKaisuHeichi;   // 平地着回数
      public CHAKUKAISU6_INFO ChakuKaisuSyogai;   // 障害着回数
      public CHAKUKAISU6_INFO[] ChakuKaisuJyo;    // 競馬場別着回数
      public CHAKUKAISU6_INFO[] ChakuKaisuKyori;  // 距離別着回数

      // 配列の初期化

      public void Initialize()
      {
        ChakuKaisuJyo = new CHAKUKAISU6_INFO[20];
        ChakuKaisuKyori = new CHAKUKAISU6_INFO[6];
      }

      // データセット
      public void SetDataB(byte[] bBuff)
      {
        Initialize();
        int i;

        SetYear = MidB2S(ref bBuff, 1, 4);
        HonSyokinHeichi = MidB2S(ref bBuff, 5, 10);
        HonSyokinSyogai = MidB2S(ref bBuff, 15, 10);
        FukaSyokinHeichi = MidB2S(ref bBuff, 25, 10);
        FukaSyokinSyogai = MidB2S(ref bBuff, 35, 10);
        ChakuKaisuHeichi.SetDataB(MidB2B(ref bBuff, 45, 36));
        ChakuKaisuSyogai.SetDataB(MidB2B(ref bBuff, 81, 36));

        for (i = 0; i < 20; i++)
        {
          ChakuKaisuJyo[i].SetDataB(MidB2B(ref bBuff, 117 + (36 * i), 36));
        }
        for (i = 0; i < 6; i++)
        {
          ChakuKaisuKyori[i].SetDataB(MidB2B(ref bBuff, 837 + (36 * i), 36));
        }

      }
    }

    /// <summary>
    /// レース情報
    /// </summary>
    public struct RACE_INFO
    {
      public string YoubiCD;      // 曜日コード

      public string TokuNum;      // 特別競走番号
      public string Hondai;       // 競走名本題

      public string Fukudai;      // 競走名副題

      public string Kakko;        // 競走名カッコ内

      public string HondaiEng;    // 競走名本題欧字

      public string FukudaiEng;   // 競走名副題欧字

      public string KakkoEng;     // 競走名カッコ内欧字

      public string Ryakusyo10;   // 競走名略称１０字

      public string Ryakusyo6;    // 競走名略称６字

      public string Ryakusyo3;    // 競走名略称３字

      public string Kubun;        // 競走名区分

      public string Nkai;         // 重賞回次[第N回]

      // データセット
      public void SetDataB(byte[] bBuff)
      {
        YoubiCD = MidB2S(ref bBuff, 1, 1);
        TokuNum = MidB2S(ref bBuff, 2, 4);
        Hondai = MidB2S(ref bBuff, 6, 60);
        Fukudai = MidB2S(ref bBuff, 66, 60);
        Kakko = MidB2S(ref bBuff, 126, 60);
        HondaiEng = MidB2S(ref bBuff, 186, 120);
        FukudaiEng = MidB2S(ref bBuff, 306, 120);
        KakkoEng = MidB2S(ref bBuff, 426, 120);
        Ryakusyo10 = MidB2S(ref bBuff, 546, 20);
        Ryakusyo6 = MidB2S(ref bBuff, 566, 12);
        Ryakusyo3 = MidB2S(ref bBuff, 578, 6);
        Kubun = MidB2S(ref bBuff, 584, 1);
        Nkai = MidB2S(ref bBuff, 585, 3);
      }
    }

    /// <summary>
    /// 天候・馬場状態

    /// </summary>
    public struct TENKO_BABA_INFO
    {
      public string TenkoCD;      // 天候コード

      public string SibaBabaCD;   // 芝馬場状態コード

      public string DirtBabaCD;   // ダート馬場状態コード


      // データセット
      public void SetDataB(byte[] bBuff)
      {
        TenkoCD = MidB2S(ref bBuff, 1, 1);
        SibaBabaCD = MidB2S(ref bBuff, 2, 1);
        DirtBabaCD = MidB2S(ref bBuff, 3, 1);
      }
    }

    /// <summary>
    /// 着回数(サイズ3byte)
    /// </summary>
    public struct CHAKUKAISU3_INFO
    {
      public string[] ChakuKaisu;

      // 配列の初期化

      public void Initialize()
      {
        ChakuKaisu = new string[6];
      }

      // データセット
      public void SetDataB(byte[] bBuff)
      {
        Initialize();

        for (int i = 0; i < 6; i++)
        {
          ChakuKaisu[i] = MidB2S(ref bBuff, 1 + (3 * i), 3);
        }
      }
    }

    /// <summary>
    /// 着回数(サイズ4byte)
    /// </summary>
    public struct CHAKUKAISU4_INFO
    {
      public string[] ChakuKaisu;

      // 配列の初期化

      public void Initialize()
      {
        ChakuKaisu = new string[6];
      }

      // データセット
      public void SetDataB(byte[] bBuff)
      {
        Initialize();

        for (int i = 0; i < 6; i++)
        {
          ChakuKaisu[i] = MidB2S(ref bBuff, 1 + (4 * i), 4);
        }
      }
    }

    /// <summary>
    /// 着回数(サイズ5byte)
    /// </summary>
    public struct CHAKUKAISU5_INFO
    {
      public string[] ChakuKaisu;

      // 配列の初期化

      public void Initialize()
      {
        ChakuKaisu = new string[6];
      }

      // データセット
      public void SetDataB(byte[] bBuff)
      {
        Initialize();

        for (int i = 0; i < 6; i++)
        {
          ChakuKaisu[i] = MidB2S(ref bBuff, 1 + (5 * i), 5);
        }
      }
    }

    /// <summary>
    /// 着回数(サイズ6byte)
    /// </summary>
    public struct CHAKUKAISU6_INFO
    {
      public string[] ChakuKaisu;

      // 配列の初期化

      public void Initialize()
      {
        ChakuKaisu = new string[6];
      }

      // データセット
      public void SetDataB(byte[] bBuff)
      {
        Initialize();

        for (int i = 0; i < 6; i++)
        {
          ChakuKaisu[i] = MidB2S(ref bBuff, 1 + (6 * i), 6);
        }
      }
    }

    /// <summary>
    /// 競走条件コード

    /// </summary>
    public struct RACE_JYOKEN
    {
      public string SyubetuCD;    // 競走種別コード

      public string KigoCD;       // 競走記号コード

      public string JyuryoCD;     // 重量種別コード

      public string[] JyokenCD;   // 競走条件コード


      // 配列の初期化

      public void Initialize()
      {
        JyokenCD = new string[5];
      }

      // データセット
      public void SetDataB(byte[] bBuff)
      {
        Initialize();
        SyubetuCD = MidB2S(ref bBuff, 1, 2);
        KigoCD = MidB2S(ref bBuff, 3, 3);
        JyuryoCD = MidB2S(ref bBuff, 6, 1);

        for (int i = 0; i < 5; i++)
        {
          JyokenCD[i] = MidB2S(ref bBuff, 7 + (3 * i), 3);
        }
      }
    }

    #endregion

    #region データ構造体


    #region 1.特別登録馬

    /// <summary>
    /// 登録馬毎情報
    /// </summary>
    public struct TOKUUMA_INFO
    {
      public string Num;                  // 連番
      public string KettoNum;             // 血統登録番号
      public string Bamei;                // 馬名

      public string UmaKigoCD;            // 馬記号コード

      public string SexCD;                // 性別コード

      public string TozaiCD;              // 調教師東西所属コード

      public string ChokyosiCode;         // 調教師コード

      public string ChokyosiRyakusyo;     // 調教師名略称
      public string Futan;                // 負担重量

      public string Koryu;                // 交流区分


      // データセット
      public void SetDataB(byte[] bBuff)
      {
        Num = MidB2S(ref bBuff, 1, 3);
        KettoNum = MidB2S(ref bBuff, 4, 10);
        Bamei = MidB2S(ref bBuff, 14, 36);
        UmaKigoCD = MidB2S(ref bBuff, 50, 2);
        SexCD = MidB2S(ref bBuff, 52, 1);
        TozaiCD = MidB2S(ref bBuff, 53, 1);
        ChokyosiCode = MidB2S(ref bBuff, 54, 5);
        ChokyosiRyakusyo = MidB2S(ref bBuff, 59, 8);
        Futan = MidB2S(ref bBuff, 67, 3);
        Koryu = MidB2S(ref bBuff, 70, 1);
      }
    }

    public struct JV_TK_TOKUUMA
    {
      public RECORD_ID head;              // <レコードヘッダー>
      public RACE_ID id;                  // <競走識別情報>
      public RACE_INFO RaceInfo;          // <レース情報>
      public string GradeCD;              // グレードコード

      public RACE_JYOKEN JyokenInfo;      // <競走条件コード>
      public string Kyori;                // 距離
      public string TrackCD;              // トラックコード

      public string CourseKubunCD;        // コース区分

      public YMD HandiDate;               // ハンデ発表日
      public string TorokuTosu;           // 登録頭数
      public TOKUUMA_INFO[] TokuUmaInfo;  // <登録馬毎情報>
      public string crlf;                 // レコード区切


      // 配列の初期化

      public void Initialize()
      {
        TokuUmaInfo = new TOKUUMA_INFO[300];
      }

      // データセット
      public void SetDataB(ref string strBuff)
      {
        Initialize();
        byte[] bBuff = new byte[21657];
        bBuff = Str2Byte(ref strBuff);

        head.SetDataB(MidB2B(ref bBuff, 1, 11));
        id.SetDataB(MidB2B(ref bBuff, 12, 16));
        RaceInfo.SetDataB(MidB2B(ref bBuff, 28, 587));
        GradeCD = MidB2S(ref bBuff, 615, 1);
        JyokenInfo.SetDataB(MidB2B(ref bBuff, 616, 21));
        Kyori = MidB2S(ref bBuff, 637, 4);
        TrackCD = MidB2S(ref bBuff, 641, 2);
        CourseKubunCD = MidB2S(ref bBuff, 643, 2);
        HandiDate.SetDataB(MidB2B(ref bBuff, 645, 8));
        TorokuTosu = MidB2S(ref bBuff, 653, 3);

        for (int i = 0; i < 300; i++)
        {
          TokuUmaInfo[i].SetDataB(MidB2B(ref bBuff, 656 + (70 * i), 70));
        }

        crlf = MidB2S(ref bBuff, 21656, 2);
        bBuff = null;
      }
    }

    #endregion

    #region 2.レース詳細

    /// <summary>
    /// コーナー通過順位

    /// </summary>
    public struct CORNER_INFO
    {
      public string Corner;       // コーナー
      public string Syukaisu;     // 周回数
      public string Jyuni;        // 各通過順位


      // データセット
      public void SetDataB(byte[] bBuff)
      {
        Corner = MidB2S(ref bBuff, 1, 1);
        Syukaisu = MidB2S(ref bBuff, 2, 1);
        Jyuni = MidB2S(ref bBuff, 3, 70);
      }
    }

    public struct JV_RA_RACE
    {
      public RECORD_ID head;              // <レコードヘッダー>
      public RACE_ID id;                  // <競走識別情報>
      public RACE_INFO RaceInfo;          // <レース情報>
      public string GradeCD;              // グレードコード

      public string GradeCDBefore;        // 変更前グレードコード

      public RACE_JYOKEN JyokenInfo;      // <競走条件コード>
      public string JyokenName;           // 競走条件名称
      public string Kyori;                // 距離
      public string KyoriBefore;          // 変更前距離
      public string TrackCD;              // トラックコード

      public string TrackCDBefore;        // 変更前トラックコード

      public string CourseKubunCD;        // コース区分

      public string CourseKubunCDBefore;  // 変更前コース区分

      public string[] Honsyokin;          // 本賞金
      public string[] HonsyokinBefore;    // 変更前本賞金
      public string[] Fukasyokin;         // 付加賞金
      public string[] FukasyokinBefore;   // 変更前付加賞金
      public string HassoTime;            // 発走時刻
      public string HassoTimeBefore;      // 変更前発走時刻
      public string TorokuTosu;           // 登録頭数
      public string SyussoTosu;           // 出走頭数
      public string NyusenTosu;           // 入線頭数
      public TENKO_BABA_INFO TenkoBaba;   // 天候・馬場状態コード

      public string[] LapTime;            // ラップタイム
      public string SyogaiMileTime;       // 障害マイルタイム
      public string HaronTimeS3;          // 前３ハロンタイム
      public string HaronTimeS4;          // 前４ハロンタイム
      public string HaronTimeL3;          // 後３ハロンタイム
      public string HaronTimeL4;          // 後４ハロンタイム
      public CORNER_INFO[] CornerInfo;    // <コーナー通過順位>
      public string RecordUpKubun;        // レコード更新区分

      public string crlf;                 // レコード区切り

      // 配列の初期化

      public void Initialize()
      {
        Honsyokin = new string[7];
        HonsyokinBefore = new string[5];
        Fukasyokin = new string[5];
        FukasyokinBefore = new string[3];
        LapTime = new string[25];
        CornerInfo = new CORNER_INFO[4];
      }

      // データセット
      public void SetDataB(ref string strBuff)
      {
        Initialize();
        int i;
        byte[] bBuff = new byte[1272];
        bBuff = Str2Byte(ref strBuff);

        head.SetDataB(MidB2B(ref bBuff, 1, 11));
        id.SetDataB(MidB2B(ref bBuff, 12, 16));
        RaceInfo.SetDataB(MidB2B(ref bBuff, 28, 587));
        GradeCD = MidB2S(ref bBuff, 615, 1);
        GradeCDBefore = MidB2S(ref bBuff, 616, 1);
        JyokenInfo.SetDataB(MidB2B(ref bBuff, 617, 21));
        JyokenName = MidB2S(ref bBuff, 638, 60);
        Kyori = MidB2S(ref bBuff, 698, 4);
        KyoriBefore = MidB2S(ref bBuff, 702, 4);
        TrackCD = MidB2S(ref bBuff, 706, 2);
        TrackCDBefore = MidB2S(ref bBuff, 708, 2);
        CourseKubunCD = MidB2S(ref bBuff, 710, 2);
        CourseKubunCDBefore = MidB2S(ref bBuff, 712, 2);

        for (i = 0; i < 7; i++)
        {
          Honsyokin[i] = MidB2S(ref bBuff, 714 + (8 * i), 8);
        }
        for (i = 0; i < 5; i++)
        {
          HonsyokinBefore[i] = MidB2S(ref bBuff, 770 + (8 * i), 8);
        }
        for (i = 0; i < 5; i++)
        {
          Fukasyokin[i] = MidB2S(ref bBuff, 810 + (8 * i), 8);
        }
        for (i = 0; i < 3; i++)
        {
          FukasyokinBefore[i] = MidB2S(ref bBuff, 850 + (8 * i), 8);
        }

        HassoTime = MidB2S(ref bBuff, 874, 4);
        HassoTimeBefore = MidB2S(ref bBuff, 878, 4);
        TorokuTosu = MidB2S(ref bBuff, 882, 2);
        SyussoTosu = MidB2S(ref bBuff, 884, 2);
        NyusenTosu = MidB2S(ref bBuff, 886, 2);
        TenkoBaba.SetDataB(MidB2B(ref bBuff, 888, 3));

        for (i = 0; i < 25; i++)
        {
          LapTime[i] = MidB2S(ref bBuff, 891 + (3 * i), 3);
        }

        SyogaiMileTime = MidB2S(ref bBuff, 966, 4);
        HaronTimeS3 = MidB2S(ref bBuff, 970, 3);
        HaronTimeS4 = MidB2S(ref bBuff, 973, 3);
        HaronTimeL3 = MidB2S(ref bBuff, 976, 3);
        HaronTimeL4 = MidB2S(ref bBuff, 979, 3);

        for (i = 0; i < 4; i++)
        {
          CornerInfo[i].SetDataB(MidB2B(ref bBuff, 982 + (72 * i), 72));
        }

        RecordUpKubun = MidB2S(ref bBuff, 1270, 1);
        crlf = MidB2S(ref bBuff, 1271, 2);
        bBuff = null;
      }
    }

    #endregion

    #region 3.馬毎レース情報

    /// <summary>
    /// 1着馬(相手馬)情報
    /// </summary>
    public struct CHAKUUMA_INFO
    {
      public string KettoNum;     // 血統登録番号
      public string Bamei;        // 馬名


      // データセット
      public void SetDataB(byte[] bBuff)
      {
        KettoNum = MidB2S(ref bBuff, 1, 10);
        Bamei = MidB2S(ref bBuff, 11, 36);
      }
    }

    public struct JV_SE_RACE_UMA
    {
      public RECORD_ID head;                  // <レコードヘッダー>
      public RACE_ID id;                      // <競走識別情報>
      public string Wakuban;                  // 枠番
      public string Umaban;                   // 馬番
      public string KettoNum;                 // 血統登録番号
      public string Bamei;                    // 馬名

      public string UmaKigoCD;                // 馬記号コード

      public string SexCD;                    // 性別コード

      public string HinsyuCD;                 // 品種コード

      public string KeiroCD;                  // 毛色コード

      public string Barei;                    // 馬齢
      public string TozaiCD;                  // 東西所属コード

      public string ChokyosiCode;             // 調教師コード

      public string ChokyosiRyakusyo;         // 調教師名略称
      public string BanusiCode;               // 馬主コード

      public string BanusiName;               // 馬主名

      public string Fukusyoku;                // 服色標示
      public string reserved1;                // 予備
      public string Futan;                    // 負担重量

      public string FutanBefore;              // 変更前負担重量

      public string Blinker;                  // ブリンカー使用区分

      public string reserved2;                // 予備
      public string KisyuCode;                // 騎手コード

      public string KisyuCodeBefore;          // 変更前騎手コード

      public string KisyuRyakusyo;            // 騎手名略称
      public string KisyuRyakusyoBefore;      // 変更前騎手名略称
      public string MinaraiCD;                // 騎手見習コード

      public string MinaraiCDBefore;          // 変更前騎手見習コード

      public string BaTaijyu;                 // 馬体重
      public string ZogenFugo;                // 増減符号
      public string ZogenSa;                  // 増減差
      public string IJyoCD;                   // 異常区分コード

      public string NyusenJyuni;              // 入線順位

      public string KakuteiJyuni;             // 確定着順

      public string DochakuKubun;             // 同着区分

      public string DochakuTosu;              // 同着頭数
      public string Time;                     // 走破タイム
      public string ChakusaCD;                // 着差コード

      public string ChakusaCDP;               // +着差コード

      public string ChakusaCDPP;              // ++着差コード

      public string Jyuni1c;                  // 1コーナーでの順位

      public string Jyuni2c;                  // 2コーナーでの順位

      public string Jyuni3c;                  // 3コーナーでの順位

      public string Jyuni4c;                  // 4コーナーでの順位

      public string Odds;                     // 単勝オッズ
      public string Ninki;                    // 単勝人気順

      public string Honsyokin;                // 獲得本賞金
      public string Fukasyokin;               // 獲得付加賞金
      public string reserved3;                // 予備
      public string reserved4;                // 予備
      public string HaronTimeL4;              // 後４ハロンタイム
      public string HaronTimeL3;              // 後３ハロンタイム
      public CHAKUUMA_INFO[] ChakuUmaInfo;    // <1着馬(相手馬)情報>
      public string TimeDiff;                 // タイム差
      public string RecordUpKubun;            // レコード更新区分

      public string DMKubun;                  // マイニング区分

      public string DMTime;                   // マイニング予想走破タイム
      public string DMGosaP;                  // 予測誤差(信頼度)＋

      public string DMGosaM;                  // 予測誤差(信頼度)－

      public string DMJyuni;                  // マイニング予想順位

      public string KyakusituKubun;           // 今回レース脚質判定

      public string crlf;                     // レコード区切り

      // 配列の初期化

      public void Initialize()
      {
        ChakuUmaInfo = new CHAKUUMA_INFO[3];
      }

      // データセット
      public void SetDataB(ref string strBuff)
      {
        Initialize();
        int i;
        byte[] bBuff = new byte[555];
        bBuff = Str2Byte(ref strBuff);

        head.SetDataB(MidB2B(ref bBuff, 1, 11));
        id.SetDataB(MidB2B(ref bBuff, 12, 16));
        Wakuban = MidB2S(ref bBuff, 28, 1);
        Umaban = MidB2S(ref bBuff, 29, 2);
        KettoNum = MidB2S(ref bBuff, 31, 10);
        Bamei = MidB2S(ref bBuff, 41, 36);
        UmaKigoCD = MidB2S(ref bBuff, 77, 2);
        SexCD = MidB2S(ref bBuff, 79, 1);
        HinsyuCD = MidB2S(ref bBuff, 80, 1);
        KeiroCD = MidB2S(ref bBuff, 81, 2);
        Barei = MidB2S(ref bBuff, 83, 2);
        TozaiCD = MidB2S(ref bBuff, 85, 1);
        ChokyosiCode = MidB2S(ref bBuff, 86, 5);
        ChokyosiRyakusyo = MidB2S(ref bBuff, 91, 8);
        BanusiCode = MidB2S(ref bBuff, 99, 6);
        BanusiName = MidB2S(ref bBuff, 105, 64);
        Fukusyoku = MidB2S(ref bBuff, 169, 60);
        reserved1 = MidB2S(ref bBuff, 229, 60);
        Futan = MidB2S(ref bBuff, 289, 3);
        FutanBefore = MidB2S(ref bBuff, 292, 3);
        Blinker = MidB2S(ref bBuff, 295, 1);
        reserved2 = MidB2S(ref bBuff, 296, 1);
        KisyuCode = MidB2S(ref bBuff, 297, 5);
        KisyuCodeBefore = MidB2S(ref bBuff, 302, 5);
        KisyuRyakusyo = MidB2S(ref bBuff, 307, 8);
        KisyuRyakusyoBefore = MidB2S(ref bBuff, 315, 8);
        MinaraiCD = MidB2S(ref bBuff, 323, 1);
        MinaraiCDBefore = MidB2S(ref bBuff, 324, 1);
        BaTaijyu = MidB2S(ref bBuff, 325, 3);
        ZogenFugo = MidB2S(ref bBuff, 328, 1);
        ZogenSa = MidB2S(ref bBuff, 329, 3);
        IJyoCD = MidB2S(ref bBuff, 332, 1);
        NyusenJyuni = MidB2S(ref bBuff, 333, 2);
        KakuteiJyuni = MidB2S(ref bBuff, 335, 2);
        DochakuKubun = MidB2S(ref bBuff, 337, 1);
        DochakuTosu = MidB2S(ref bBuff, 338, 1);
        Time = MidB2S(ref bBuff, 339, 4);
        ChakusaCD = MidB2S(ref bBuff, 343, 3);
        ChakusaCDP = MidB2S(ref bBuff, 346, 3);
        ChakusaCDPP = MidB2S(ref bBuff, 349, 3);
        Jyuni1c = MidB2S(ref bBuff, 352, 2);
        Jyuni2c = MidB2S(ref bBuff, 354, 2);
        Jyuni3c = MidB2S(ref bBuff, 356, 2);
        Jyuni4c = MidB2S(ref bBuff, 358, 2);
        Odds = MidB2S(ref bBuff, 360, 4);
        Ninki = MidB2S(ref bBuff, 364, 2);
        Honsyokin = MidB2S(ref bBuff, 366, 8);
        Fukasyokin = MidB2S(ref bBuff, 374, 8);
        reserved3 = MidB2S(ref bBuff, 382, 3);
        reserved4 = MidB2S(ref bBuff, 385, 3);
        HaronTimeL4 = MidB2S(ref bBuff, 388, 3);
        HaronTimeL3 = MidB2S(ref bBuff, 391, 3);

        for (i = 0; i < 3; i++)
        {
          ChakuUmaInfo[i].SetDataB(MidB2B(ref bBuff, 394 + (46 * i), 46));
        }

        TimeDiff = MidB2S(ref bBuff, 532, 4);
        RecordUpKubun = MidB2S(ref bBuff, 536, 1);
        DMKubun = MidB2S(ref bBuff, 537, 1);
        DMTime = MidB2S(ref bBuff, 538, 5);
        DMGosaP = MidB2S(ref bBuff, 543, 4);
        DMGosaM = MidB2S(ref bBuff, 547, 4);
        DMJyuni = MidB2S(ref bBuff, 551, 2);
        KyakusituKubun = MidB2S(ref bBuff, 553, 1);
        crlf = MidB2S(ref bBuff, 554, 2);
        bBuff = null;
      }
    }

    #endregion

    #region 4.払戻

    /// <summary>
    /// 払戻情報1 単・複・枠
    /// </summary>
    public struct PAY_INFO1
    {
      public string Umaban;   // 馬番
      public string Pay;      // 払戻金

      public string Ninki;    // 人気順


      // データセット
      public void SetDataB(byte[] bBuff)
      {
        Umaban = MidB2S(ref bBuff, 1, 2);
        Pay = MidB2S(ref bBuff, 3, 9);
        Ninki = MidB2S(ref bBuff, 12, 2);
      }
    }

    /// <summary>
    /// 払戻情報2 馬連・ワイド・予備・馬単

    /// </summary>
    public struct PAY_INFO2
    {
      public string Kumi;     // 組番
      public string Pay;      // 払戻金

      public string Ninki;    // 人気順


      // データセット
      public void SetDataB(byte[] bBuff)
      {
        Kumi = MidB2S(ref bBuff, 1, 4);
        Pay = MidB2S(ref bBuff, 5, 9);
        Ninki = MidB2S(ref bBuff, 14, 3);
      }
    }

    /// <summary>
    /// 払戻情報3 3連複

    /// </summary>
    public struct PAY_INFO3
    {
      public string Kumi;     // 組番
      public string Pay;      // 払戻金

      public string Ninki;    // 人気順


      // データセット
      public void SetDataB(byte[] bBuff)
      {
        Kumi = MidB2S(ref bBuff, 1, 6);
        Pay = MidB2S(ref bBuff, 7, 9);
        Ninki = MidB2S(ref bBuff, 16, 3);
      }
    }

    /// <summary>
    /// 払戻情報4 3連単

    /// </summary>
    public struct PAY_INFO4
    {
      public string Kumi;     // 組番
      public string Pay;      // 払戻金

      public string Ninki;    // 人気順


      // データセット
      public void SetDataB(byte[] bBuff)
      {
        Kumi = MidB2S(ref bBuff, 1, 6);
        Pay = MidB2S(ref bBuff, 7, 9);
        Ninki = MidB2S(ref bBuff, 16, 4);
      }
    }

    public struct JV_HR_PAY
    {
      public RECORD_ID head;              // <レコードヘッダー>
      public RACE_ID id;                  // <競走識別情報>
      public string TorokuTosu;     // 登録頭数
      public string SyussoTosu;           // 出走頭数
      public string[] FuseirituFlag;      // 不成立フラグ
      public string[] TokubaraiFlag;      // 特払フラグ
      public string[] HenkanFlag;         // 返還フラグ
      public string[] HenkanUma;          // 返還馬番情報(馬番01～28)
      public string[] HenkanWaku;         // 返還枠番情報(枠番1～8)
      public string[] HenkanDoWaku;   // 返還同枠情報(枠番1～8)
      public PAY_INFO1[] PayTansyo;       // <単勝払戻>
      public PAY_INFO1[] PayFukusyo;      // <複勝払戻>
      public PAY_INFO1[] PayWakuren;      // <枠連払戻>
      public PAY_INFO2[] PayUmaren;       // <馬連払戻>
      public PAY_INFO2[] PayWide;         // <ワイド払戻>
      public PAY_INFO2[] PayReserved1;    // <予備>
      public PAY_INFO2[] PayUmatan;       // <馬単払戻>
      public PAY_INFO3[] PaySanrenpuku;   // <3連複払戻>
      public PAY_INFO4[] PaySanrentan;    // <3連単払戻>
      public string crlf;                 // レコード区切り

      // 配列の初期化

      public void Initialize()
      {
        FuseirituFlag = new string[9];
        TokubaraiFlag = new string[9];
        HenkanFlag = new string[9];
        HenkanUma = new string[28];
        HenkanWaku = new string[8];
        HenkanDoWaku = new string[8];
        PayTansyo = new PAY_INFO1[3];
        PayFukusyo = new PAY_INFO1[5];
        PayWakuren = new PAY_INFO1[3];
        PayUmaren = new PAY_INFO2[3];
        PayWide = new PAY_INFO2[7];
        PayReserved1 = new PAY_INFO2[3];
        PayUmatan = new PAY_INFO2[6];
        PaySanrenpuku = new PAY_INFO3[3];
        PaySanrentan = new PAY_INFO4[6];
      }

      // データセット
      public void SetDataB(ref string strBuff)
      {
        Initialize();
        int i;
        byte[] bBuff = new byte[719];
        bBuff = Str2Byte(ref strBuff);

        head.SetDataB(MidB2B(ref bBuff, 1, 11));
        id.SetDataB(MidB2B(ref bBuff, 12, 16));
        TorokuTosu = MidB2S(ref bBuff, 28, 2);
        SyussoTosu = MidB2S(ref bBuff, 30, 2);

        for (i = 0; i < 9; i++)
        {
          FuseirituFlag[i] = MidB2S(ref bBuff, 32 + (1 * i), 1);
        }
        for (i = 0; i < 9; i++)
        {
          TokubaraiFlag[i] = MidB2S(ref bBuff, 41 + (1 * i), 1);
        }
        for (i = 0; i < 9; i++)
        {
          HenkanFlag[i] = MidB2S(ref bBuff, 50 + (1 * i), 1);
        }
        for (i = 0; i < 28; i++)
        {
          HenkanUma[i] = MidB2S(ref bBuff, 59 + (1 * i), 1);
        }
        for (i = 0; i < 8; i++)
        {
          HenkanWaku[i] = MidB2S(ref bBuff, 87 + (1 * i), 1);
        }
        for (i = 0; i < 8; i++)
        {
          HenkanDoWaku[i] = MidB2S(ref bBuff, 95 + (1 * i), 1);
        }
        for (i = 0; i < 3; i++)
        {
          PayTansyo[i].SetDataB(MidB2B(ref bBuff, 103 + (13 * i), 13));
        }
        for (i = 0; i < 5; i++)
        {
          PayFukusyo[i].SetDataB(MidB2B(ref bBuff, 142 + (13 * i), 13));
        }
        for (i = 0; i < 3; i++)
        {
          PayWakuren[i].SetDataB(MidB2B(ref bBuff, 207 + (13 * i), 13));
        }
        for (i = 0; i < 3; i++)
        {
          PayUmaren[i].SetDataB(MidB2B(ref bBuff, 246 + (16 * i), 16));
        }
        for (i = 0; i < 7; i++)
        {
          PayWide[i].SetDataB(MidB2B(ref bBuff, 294 + (16 * i), 16));
        }
        for (i = 0; i < 3; i++)
        {
          PayReserved1[i].SetDataB(MidB2B(ref bBuff, 406 + (16 * i), 16));
        }
        for (i = 0; i < 6; i++)
        {
          PayUmatan[i].SetDataB(MidB2B(ref bBuff, 454 + (16 * i), 16));
        }
        for (i = 0; i < 3; i++)
        {
          PaySanrenpuku[i].SetDataB(MidB2B(ref bBuff, 550 + (18 * i), 18));
        }
        for (i = 0; i < 6; i++)
        {
          PaySanrentan[i].SetDataB(MidB2B(ref bBuff, 604 + (19 * i), 19));
        }

        crlf = MidB2S(ref bBuff, 718, 2);
        bBuff = null;
      }
    }

    #endregion

    #region 5.票数(全掛式)

    /// <summary>
    /// 票数情報1 単・複・枠
    /// </summary>
    public struct HYO_INFO1
    {
      public string Umaban;   // 馬番
      public string Hyo;      // 票数
      public string Ninki;    // 人気


      // データセット
      public void SetDataB(byte[] bBuff)
      {
        Umaban = MidB2S(ref bBuff, 1, 2);
        Hyo = MidB2S(ref bBuff, 3, 11);
        Ninki = MidB2S(ref bBuff, 14, 2);
      }
    }

    /// <summary>
    /// 票数情報2 馬連・ワイド・馬単

    /// </summary>
    public struct HYO_INFO2
    {
      public string Kumi;     // 馬番
      public string Hyo;      // 票数
      public string Ninki;    // 人気


      // データセット
      public void SetDataB(byte[] bBuff)
      {
        Kumi = MidB2S(ref bBuff, 1, 4);
        Hyo = MidB2S(ref bBuff, 5, 11);
        Ninki = MidB2S(ref bBuff, 16, 3);
      }
    }

    /// <summary>
    /// 票数情報3 3連複票数
    /// </summary>
    public struct HYO_INFO3
    {
      public string Kumi;     // 馬番
      public string Hyo;      // 票数
      public string Ninki;    // 人気


      // データセット
      public void SetDataB(byte[] bBuff)
      {
        Kumi = MidB2S(ref bBuff, 1, 6);
        Hyo = MidB2S(ref bBuff, 7, 11);
        Ninki = MidB2S(ref bBuff, 18, 3);
      }
    }

    /// <summary>
    /// 票数情報4 3連単票数
    /// </summary>
    public struct HYO_INFO4
    {
      public string Kumi;     // 馬番
      public string Hyo;      // 票数
      public string Ninki;    // 人気


      // データセット
      public void SetDataB(byte[] bBuff)
      {
        Kumi = MidB2S(ref bBuff, 1, 6);
        Hyo = MidB2S(ref bBuff, 7, 11);
        Ninki = MidB2S(ref bBuff, 18, 4);
      }
    }

    public struct JV_H1_HYOSU_ZENKAKE
    {
      public RECORD_ID head;              // <レコードヘッダー>
      public RACE_ID id;                  // <競走識別情報>
      public string TorokuTosu;     // 登録頭数
      public string SyussoTosu;           // 出走頭数
      public string[] HatubaiFlag;    // 発売フラグ
      public string FukuChakuBaraiKey;  // 複勝着払キー
      public string[] HenkanUma;        // 返還馬番情報(馬番01～28)
      public string[] HenkanWaku;       // 返還枠番情報(枠番1～8)
      public string[] HenkanDoWaku;   // 返還同枠情報(枠番1～8)
      public HYO_INFO1[] HyoTansyo;   // <単勝票数>
      public HYO_INFO1[] HyoFukusyo;      // <複勝票数>
      public HYO_INFO1[] HyoWakuren;      // <枠連票数>
      public HYO_INFO2[] HyoUmaren;   // <馬連票数>
      public HYO_INFO2[] HyoWide;       // <ワイド票数>
      public HYO_INFO2[] HyoUmatan;   // <馬単票数>
      public HYO_INFO3[] HyoSanrenpuku; // <3連複票数>
      public string[] HyoTotal;     // 票数合計

      public string crlf;           // レコード区切り

      // 配列の初期化

      public void Initialize()
      {
        HatubaiFlag = new string[7];
        HenkanUma = new string[28];
        HenkanWaku = new string[8];
        HenkanDoWaku = new string[8];
        HyoTansyo = new HYO_INFO1[28];
        HyoFukusyo = new HYO_INFO1[28];
        HyoWakuren = new HYO_INFO1[36];
        HyoUmaren = new HYO_INFO2[153];
        HyoWide = new HYO_INFO2[153];
        HyoUmatan = new HYO_INFO2[306];
        HyoSanrenpuku = new HYO_INFO3[816];
        HyoTotal = new string[14];
      }

      // データセット
      public void SetDataB(ref string strBuff)
      {
        Initialize();
        int i;
        byte[] bBuff = new byte[28955];
        bBuff = Str2Byte(ref strBuff);

        head.SetDataB(MidB2B(ref bBuff, 1, 11));
        id.SetDataB(MidB2B(ref bBuff, 12, 16));
        TorokuTosu = MidB2S(ref bBuff, 28, 2);
        SyussoTosu = MidB2S(ref bBuff, 30, 2);

        for (i = 0; i < 7; i++)
        {
          HatubaiFlag[i] = MidB2S(ref bBuff, 32 + (1 * i), 1);
        }

        FukuChakuBaraiKey = MidB2S(ref bBuff, 39, 1);

        for (i = 0; i < 28; i++)
        {
          HenkanUma[i] = MidB2S(ref bBuff, 40 + (1 * i), 1);
        }
        for (i = 0; i < 8; i++)
        {
          HenkanWaku[i] = MidB2S(ref bBuff, 68 + (1 * i), 1);
        }
        for (i = 0; i < 8; i++)
        {
          HenkanDoWaku[i] = MidB2S(ref bBuff, 76 + (1 * i), 1);
        }
        for (i = 0; i < 28; i++)
        {
          HyoTansyo[i].SetDataB(MidB2B(ref bBuff, 84 + (15 * i), 15));
        }
        for (i = 0; i < 28; i++)
        {
          HyoFukusyo[i].SetDataB(MidB2B(ref bBuff, 504 + (15 * i), 15));
        }
        for (i = 0; i < 36; i++)
        {
          HyoWakuren[i].SetDataB(MidB2B(ref bBuff, 924 + (15 * i), 15));
        }
        for (i = 0; i < 153; i++)
        {
          HyoUmaren[i].SetDataB(MidB2B(ref bBuff, 1464 + (18 * i), 18));
        }
        for (i = 0; i < 153; i++)
        {
          HyoWide[i].SetDataB(MidB2B(ref bBuff, 4218 + (18 * i), 18));
        }
        for (i = 0; i < 306; i++)
        {
          HyoUmatan[i].SetDataB(MidB2B(ref bBuff, 6972 + (18 * i), 18));
        }
        for (i = 0; i < 816; i++)
        {
          HyoSanrenpuku[i].SetDataB(MidB2B(ref bBuff, 12480 + (20 * i), 20));
        }
        for (i = 0; i < 14; i++)
        {
          HyoTotal[i] = MidB2S(ref bBuff, 28800 + (11 * i), 11);
        }

        crlf = MidB2S(ref bBuff, 28954, 2);
        bBuff = null;
      }
    }

    public struct JV_H6_HYOSU_SANRENTAN
    {
      public RECORD_ID head;              // <レコードヘッダー>
      public RACE_ID id;                  // <競走識別情報>
      public string TorokuTosu;     // 登録頭数
      public string SyussoTosu;           // 出走頭数
      public string HatubaiFlag;      // 発売フラグ
      public string[] HenkanUma;        // 返還馬番情報(馬番01～18)
      public HYO_INFO4[] HyoSanrentan;    // <3連単票数>
      public string[] HyoTotal;     // 票数合計

      public string crlf;           // レコード区切り

      // 配列の初期化

      public void Initialize()
      {
        HenkanUma = new string[18];
        HyoSanrentan = new HYO_INFO4[4896];
        HyoTotal = new string[2];
      }

      // データセット
      public void SetDataB(ref string strBuff)
      {
        Initialize();
        int i;
        byte[] bBuff = new byte[102900];
        bBuff = Str2Byte(ref strBuff);

        head.SetDataB(MidB2B(ref bBuff, 1, 11));
        id.SetDataB(MidB2B(ref bBuff, 12, 16));
        TorokuTosu = MidB2S(ref bBuff, 28, 2);
        SyussoTosu = MidB2S(ref bBuff, 30, 2);
        HatubaiFlag = MidB2S(ref bBuff, 32, 1);

        for (i = 0; i < 18; i++)
        {
          HenkanUma[i] = MidB2S(ref bBuff, 33 + (1 * i), 1);
        }
        for (i = 0; i < 4896; i++)
        {
          HyoSanrentan[i].SetDataB(MidB2B(ref bBuff, 51 + (21 * i), 21));
        }
        for (i = 0; i < 2; i++)
        {
          HyoTotal[i] = MidB2S(ref bBuff, 102867 + (11 * i), 11);
        }

        crlf = MidB2S(ref bBuff, 102889, 2);
        bBuff = null;
      }
    }

    #endregion

    #region 6.オッズ(単複枠)

    /// <summary>
    /// 単勝オッズ
    /// </summary>
    public struct ODDS_TANSYO_INFO
    {
      public string Umaban;   // 馬番
      public string Odds;     // オッズ
      public string Ninki;    // 人気順


      // データセット
      public void SetDataB(byte[] bBuff)
      {
        Umaban = MidB2S(ref bBuff, 1, 2);
        Odds = MidB2S(ref bBuff, 3, 4);
        Ninki = MidB2S(ref bBuff, 7, 2);
      }
    }

    /// <summary>
    /// 複勝オッズ
    /// </summary>
    public struct ODDS_FUKUSYO_INFO
    {
      public string Umaban;       // 馬番
      public string OddsLow;      // 最低オッズ
      public string OddsHigh;     // 最高オッズ
      public string Ninki;        // 人気順


      // データセット
      public void SetDataB(byte[] bBuff)
      {
        Umaban = MidB2S(ref bBuff, 1, 2);
        OddsLow = MidB2S(ref bBuff, 3, 4);
        OddsHigh = MidB2S(ref bBuff, 7, 4);
        Ninki = MidB2S(ref bBuff, 11, 2);
      }
    }

    /// <summary>
    /// 枠連オッズ
    /// </summary>
    public struct ODDS_WAKUREN_INFO
    {
      public string Kumi;     // 組番
      public string Odds;     // オッズ
      public string Ninki;    // 人気順


      // データセット
      public void SetDataB(byte[] bBuff)
      {
        Kumi = MidB2S(ref bBuff, 1, 2);
        Odds = MidB2S(ref bBuff, 3, 5);
        Ninki = MidB2S(ref bBuff, 8, 2);
      }
    }

    public struct JV_O1_ODDS_TANFUKUWAKU
    {
      public RECORD_ID head;                          // <レコードヘッダー>
      public RACE_ID id;                              // <競走識別情報>
      public MDHM HappyoTime;                         // 発表月日時分
      public string TorokuTosu;                       // 登録頭数
      public string SyussoTosu;                       // 出走頭数
      public string TansyoFlag;                       // 発売フラグ 単勝
      public string FukusyoFlag;                      // 発売フラグ 複勝
      public string WakurenFlag;                      // 発売フラグ 枠連
      public string FukuChakuBaraiKey;                // 複勝着払キー
      public ODDS_TANSYO_INFO[] OddsTansyoInfo;       // <単勝オッズ>
      public ODDS_FUKUSYO_INFO[] OddsFukusyoInfo;     // <複勝票数オッズ>
      public ODDS_WAKUREN_INFO[] OddsWakurenInfo;     // <枠連票数オッズ>
      public string TotalHyosuTansyo;                 // 単勝票数合計

      public string TotalHyosuFukusyo;                // 複勝票数合計

      public string TotalHyosuWakuren;                // 枠連票数合計

      public string crlf;                             // レコード区切り

      // 配列の初期化

      public void Initialize()
      {
        OddsTansyoInfo = new ODDS_TANSYO_INFO[28];
        OddsFukusyoInfo = new ODDS_FUKUSYO_INFO[28];
        OddsWakurenInfo = new ODDS_WAKUREN_INFO[36];
      }

      // データセット
      public void SetDataB(ref string strBuff)
      {
        Initialize();
        int i;
        byte[] bBuff = new byte[962];
        bBuff = Str2Byte(ref strBuff);

        head.SetDataB(MidB2B(ref bBuff, 1, 11));
        id.SetDataB(MidB2B(ref bBuff, 12, 16));
        HappyoTime.SetDataB(MidB2B(ref bBuff, 28, 8));
        TorokuTosu = MidB2S(ref bBuff, 36, 2);
        SyussoTosu = MidB2S(ref bBuff, 38, 2);
        TansyoFlag = MidB2S(ref bBuff, 40, 1);
        FukusyoFlag = MidB2S(ref bBuff, 41, 1);
        WakurenFlag = MidB2S(ref bBuff, 42, 1);
        FukuChakuBaraiKey = MidB2S(ref bBuff, 43, 1);

        for (i = 0; i < 28; i++)
        {
          OddsTansyoInfo[i].SetDataB(MidB2B(ref bBuff, 44 + (8 * i), 8));
        }
        for (i = 0; i < 28; i++)
        {
          OddsFukusyoInfo[i].SetDataB(MidB2B(ref bBuff, 268 + (12 * i), 12));
        }
        for (i = 0; i < 36; i++)
        {
          OddsWakurenInfo[i].SetDataB(MidB2B(ref bBuff, 604 + (9 * i), 9));
        }

        TotalHyosuTansyo = MidB2S(ref bBuff, 928, 11);
        TotalHyosuFukusyo = MidB2S(ref bBuff, 939, 11);
        TotalHyosuWakuren = MidB2S(ref bBuff, 950, 11);
        crlf = MidB2S(ref bBuff, 961, 2);
        bBuff = null;
      }
    }

    #endregion

    #region 7.オッズ(馬連)

    /// <summary>
    /// 馬連オッズ
    /// </summary>
    public struct ODDS_UMAREN_INFO
    {
      public string Kumi;     // 組番
      public string Odds;     // オッズ
      public string Ninki;    // 人気順


      // データセット
      public void SetDataB(byte[] bBuff)
      {
        Kumi = MidB2S(ref bBuff, 1, 4);
        Odds = MidB2S(ref bBuff, 5, 6);
        Ninki = MidB2S(ref bBuff, 11, 3);
      }
    }

    public struct JV_O2_ODDS_UMAREN
    {
      public RECORD_ID head;                          // <レコードヘッダー>
      public RACE_ID id;                              // <競走識別情報>
      public MDHM HappyoTime;                         // 発表月日時分
      public string TorokuTosu;                       // 登録頭数
      public string SyussoTosu;                       // 出走頭数
      public string UmarenFlag;                       // 発売フラグ 馬連
      public ODDS_UMAREN_INFO[] OddsUmarenInfo;       // <馬連票数オッズ>
      public string TotalHyosuUmaren;                 // 馬連票数合計

      public string crlf;                             // レコード区切り

      // 配列の初期化

      public void Initialize()
      {
        OddsUmarenInfo = new ODDS_UMAREN_INFO[153];
      }

      // データセット
      public void SetDataB(ref string strBuff)
      {
        Initialize();
        byte[] bBuff = new byte[2042];
        bBuff = Str2Byte(ref strBuff);

        head.SetDataB(MidB2B(ref bBuff, 1, 11));
        id.SetDataB(MidB2B(ref bBuff, 12, 16));
        HappyoTime.SetDataB(MidB2B(ref bBuff, 28, 8));
        TorokuTosu = MidB2S(ref bBuff, 36, 2);
        SyussoTosu = MidB2S(ref bBuff, 38, 2);
        UmarenFlag = MidB2S(ref bBuff, 40, 1);

        for (int i = 0; i < 153; i++)
        {
          OddsUmarenInfo[i].SetDataB(MidB2B(ref bBuff, 41 + (13 * i), 13));
        }

        TotalHyosuUmaren = MidB2S(ref bBuff, 2030, 11);
        crlf = MidB2S(ref bBuff, 2041, 2);
        bBuff = null;
      }
    }

    #endregion

    #region 8.オッズ(ワイド)

    /// <summary>
    /// ワイドオッズ
    /// </summary>
    public struct ODDS_WIDE_INFO
    {
      public string Kumi;         // 組番
      public string OddsLow;      // 最低オッズ
      public string OddsHigh;     // 最高オッズ
      public string Ninki;        // 人気順


      // データセット
      public void SetDataB(byte[] bBuff)
      {
        Kumi = MidB2S(ref bBuff, 1, 4);
        OddsLow = MidB2S(ref bBuff, 5, 5);
        OddsHigh = MidB2S(ref bBuff, 10, 5);
        Ninki = MidB2S(ref bBuff, 15, 3);
      }
    }

    public struct JV_O3_ODDS_WIDE
    {
      public RECORD_ID head;                      // <レコードヘッダー>
      public RACE_ID id;                          // <競走識別情報>
      public MDHM HappyoTime;                     // 発表月日時分
      public string TorokuTosu;                   // 登録頭数
      public string SyussoTosu;                   // 出走頭数
      public string WideFlag;                     // 発売フラグ ワイド

      public ODDS_WIDE_INFO[] OddsWideInfo;       // <ワイド票数オッズ>
      public string TotalHyosuWide;               // ワイド票数合計

      public string crlf;                         //レコード区切り

      // 配列の初期化

      public void Initialize()
      {
        OddsWideInfo = new ODDS_WIDE_INFO[153];
      }

      // データセット
      public void SetDataB(ref string strBuff)
      {
        Initialize();
        byte[] bBuff = new byte[2654];
        bBuff = Str2Byte(ref strBuff);

        head.SetDataB(MidB2B(ref bBuff, 1, 11));
        id.SetDataB(MidB2B(ref bBuff, 12, 16));
        HappyoTime.SetDataB(MidB2B(ref bBuff, 28, 8));
        TorokuTosu = MidB2S(ref bBuff, 36, 2);
        SyussoTosu = MidB2S(ref bBuff, 38, 2);
        WideFlag = MidB2S(ref bBuff, 40, 1);

        for (int i = 0; i < 153; i++)
        {
          OddsWideInfo[i].SetDataB(MidB2B(ref bBuff, 41 + (17 * i), 17));
        }

        TotalHyosuWide = MidB2S(ref bBuff, 2642, 11);
        crlf = MidB2S(ref bBuff, 2653, 2);
        bBuff = null;
      }
    }

    #endregion

    #region 9.オッズ(馬単)

    /// <summary>
    /// 馬単オッズ
    /// </summary>
    public struct ODDS_UMATAN_INFO
    {
      public string Kumi;     // 組番
      public string Odds;     // オッズ
      public string Ninki;    // 人気順


      // データセット
      public void SetDataB(byte[] bBuff)
      {
        Kumi = MidB2S(ref bBuff, 1, 4);
        Odds = MidB2S(ref bBuff, 5, 6);
        Ninki = MidB2S(ref bBuff, 11, 3);
      }
    }

    public struct JV_O4_ODDS_UMATAN
    {
      public RECORD_ID head;                          // <レコードヘッダー>
      public RACE_ID id;                              // <競走識別情報>
      public MDHM HappyoTime;                         // 発表月日時分
      public string TorokuTosu;                       // 登録頭数
      public string SyussoTosu;                       // 出走頭数
      public string UmatanFlag;                       // 発売フラグ 馬単

      public ODDS_UMATAN_INFO[] OddsUmatanInfo;       // <馬単票数オッズ>
      public string TotalHyosuUmatan;                 // 馬単票数合計

      public string crlf;                             // レコード区切り

      // 配列の初期化

      public void Initialize()
      {
        OddsUmatanInfo = new ODDS_UMATAN_INFO[306];
      }

      // データセット
      public void SetDataB(ref string strBuff)
      {
        Initialize();
        byte[] bBuff = new byte[4031];
        bBuff = Str2Byte(ref strBuff);

        head.SetDataB(MidB2B(ref bBuff, 1, 11));
        id.SetDataB(MidB2B(ref bBuff, 12, 16));
        HappyoTime.SetDataB(MidB2B(ref bBuff, 28, 8));
        TorokuTosu = MidB2S(ref bBuff, 36, 2);
        SyussoTosu = MidB2S(ref bBuff, 38, 2);
        UmatanFlag = MidB2S(ref bBuff, 40, 1);

        for (int i = 0; i < 306; i++)
        {
          OddsUmatanInfo[i].SetDataB(MidB2B(ref bBuff, 41 + (13 * i), 13));
        }

        TotalHyosuUmatan = MidB2S(ref bBuff, 4019, 11);
        crlf = MidB2S(ref bBuff, 4030, 2);
        bBuff = null;
      }
    }

    #endregion

    #region 10.オッズ(3連複)

    /// <summary>
    /// 3連複オッズ
    /// </summary>
    public struct ODDS_SANREN_INFO
    {
      public string Kumi;     // 組番
      public string Odds;     // オッズ
      public string Ninki;    // 人気順


      // データセット
      public void SetDataB(byte[] bBuff)
      {
        Kumi = MidB2S(ref bBuff, 1, 6);
        Odds = MidB2S(ref bBuff, 7, 6);
        Ninki = MidB2S(ref bBuff, 13, 3);
      }
    }

    public struct JV_O5_ODDS_SANREN
    {
      public RECORD_ID head;                          // <レコードヘッダー>
      public RACE_ID id;                              // <競走識別情報>
      public MDHM HappyoTime;                         // 発表月日時分
      public string TorokuTosu;                       // 登録頭数
      public string SyussoTosu;                       // 出走頭数
      public string SanrenpukuFlag;                   // 発売フラグ 3連複

      public ODDS_SANREN_INFO[] OddsSanrenInfo;       // <3連複票数オッズ>
      public string TotalHyosuSanrenpuku;             // 3連複票数合計

      public string crlf;                             // レコード区切り

      // 配列の初期化

      public void Initialize()
      {
        OddsSanrenInfo = new ODDS_SANREN_INFO[816];
      }

      // データセット
      public void SetDataB(ref string strBuff)
      {
        Initialize();
        byte[] bBuff = new byte[12293];
        bBuff = Str2Byte(ref strBuff);

        head.SetDataB(MidB2B(ref bBuff, 1, 11));
        id.SetDataB(MidB2B(ref bBuff, 12, 16));
        HappyoTime.SetDataB(MidB2B(ref bBuff, 28, 8));
        TorokuTosu = MidB2S(ref bBuff, 36, 2);
        SyussoTosu = MidB2S(ref bBuff, 38, 2);
        SanrenpukuFlag = MidB2S(ref bBuff, 40, 1);

        for (int i = 0; i < 816; i++)
        {
          OddsSanrenInfo[i].SetDataB(MidB2B(ref bBuff, 41 + (15 * i), 15));
        }

        TotalHyosuSanrenpuku = MidB2S(ref bBuff, 12281, 11);
        crlf = MidB2S(ref bBuff, 12292, 2);
        bBuff = null;
      }
    }

    #endregion

    #region 10-1.オッズ(3連単)

    /// <summary>
    /// 3連単オッズ
    /// </summary>
    public struct ODDS_SANRENTAN_INFO
    {
      public string Kumi;     // 組番
      public string Odds;     // オッズ
      public string Ninki;    // 人気順


      // データセット
      public void SetDataB(byte[] bBuff)
      {
        Kumi = MidB2S(ref bBuff, 1, 6);
        Odds = MidB2S(ref bBuff, 7, 7);
        Ninki = MidB2S(ref bBuff, 14, 4);
      }
    }

    public struct JV_O6_ODDS_SANRENTAN
    {
      public RECORD_ID head;                              // <レコードヘッダー>
      public RACE_ID id;                                  // <競走識別情報>
      public MDHM HappyoTime;                             // 発表月日時分
      public string TorokuTosu;                           // 登録頭数
      public string SyussoTosu;                           // 出走頭数
      public string SanrentanFlag;                        // 発売フラグ 3連単

      public ODDS_SANRENTAN_INFO[] OddsSanrentanInfo;     // <3連単票数オッズ>
      public string TotalHyosuSanrentan;                  // 3連単票数合計

      public string crlf;                                 // レコード区切り

      // 配列の初期化

      public void Initialize()
      {
        OddsSanrentanInfo = new ODDS_SANRENTAN_INFO[4896];
      }

      // データセット
      public void SetDataB(ref string strBuff)
      {
        Initialize();
        byte[] bBuff = new byte[83285];
        bBuff = Str2Byte(ref strBuff);

        head.SetDataB(MidB2B(ref bBuff, 1, 11));
        id.SetDataB(MidB2B(ref bBuff, 12, 16));
        HappyoTime.SetDataB(MidB2B(ref bBuff, 28, 8));
        TorokuTosu = MidB2S(ref bBuff, 36, 2);
        SyussoTosu = MidB2S(ref bBuff, 38, 2);
        SanrentanFlag = MidB2S(ref bBuff, 40, 1);

        for (int i = 0; i < 4896; i++)
        {
          OddsSanrentanInfo[i].SetDataB(MidB2B(ref bBuff, 41 + (17 * i), 17));
        }

        TotalHyosuSanrentan = MidB2S(ref bBuff, 83273, 11);
        crlf = MidB2S(ref bBuff, 83284, 2);
        bBuff = null;
      }
    }

    #endregion

    #region 11.競走馬マスタ

    /// <summary>
    /// 3代血統情報
    /// </summary>
    public struct KETTO3_INFO
    {
      public string HansyokuNum;  // 繁殖登録番号
      public string Bamei;        // 馬名


      // データセット
      public void SetDataB(byte[] bBuff)
      {
        HansyokuNum = MidB2S(ref bBuff, 1, 8);
        Bamei = MidB2S(ref bBuff, 9, 36);
      }
    }

    public struct JV_UM_UMA
    {
      public RECORD_ID head;                          // <レコードヘッダー>
      public string KettoNum;                         // 血統登録番号
      public string DelKubun;                         // 競走馬抹消区分

      public YMD RegDate;                             // 競走馬登録年月日
      public YMD DelDate;                             // 競走馬抹消年月日
      public YMD BirthDate;                           // 生年月日
      public string Bamei;                            // 馬名

      public string BameiKana;                        // 馬名半角カナ

      public string BameiEng;                         // 馬名欧字

      public string ZaikyuFlag;                       // JRA施設在きゅうフラグ
      public string Reserved;                         // 予備
      public string UmaKigoCD;                        // 馬記号コード

      public string SexCD;                            // 性別コード

      public string HinsyuCD;                         // 品種コード

      public string KeiroCD;                          // 毛色コード

      public KETTO3_INFO[] Ketto3Info;                // <3代血統情報>
      public string TozaiCD;                          // 東西所属コード

      public string ChokyosiCode;                     // 調教師コード

      public string ChokyosiRyakusyo;                 // 調教師名略称
      public string Syotai;                           // 招待地域名
      public string BreederCode;                      // 生産者コード

      public string BreederName;                      // 生産者名
      public string SanchiName;                       // 産地名

      public string BanusiCode;                       // 馬主コード

      public string BanusiName;                       // 馬主名

      public string RuikeiHonsyoHeiti;                // 平地本賞金累計

      public string RuikeiHonsyoSyogai;               // 障害本賞金累計

      public string RuikeiFukaHeichi;                 // 平地付加賞金累計

      public string RuikeiFukaSyogai;                 // 障害付加賞金累計

      public string RuikeiSyutokuHeichi;              // 平地収得賞金累計

      public string RuikeiSyutokuSyogai;              // 障害収得賞金累計

      public CHAKUKAISU3_INFO ChakuSogo;              // 総合着回数
      public CHAKUKAISU3_INFO ChakuChuo;              // 中央合計着回数
      public CHAKUKAISU3_INFO[] ChakuKaisuBa;         // 馬場別着回数
      public CHAKUKAISU3_INFO[] ChakuKaisuJyotai;     // 馬場状態別着回数
      public CHAKUKAISU3_INFO[] ChakuKaisuKyori;      // 距離別着回数
      public string[] Kyakusitu;                      // 脚質傾向

      public string RaceCount;                        // 登録レース数
      public string crlf;                             // レコード区切り

      // 配列の初期化

      public void Initialize()
      {
        Ketto3Info = new KETTO3_INFO[14];
        ChakuKaisuBa = new CHAKUKAISU3_INFO[7];
        ChakuKaisuJyotai = new CHAKUKAISU3_INFO[12];
        ChakuKaisuKyori = new CHAKUKAISU3_INFO[6];
        Kyakusitu = new string[4];
      }

      // データセット
      public void SetDataB(ref string strBuff)
      {
        Initialize();
        int i;
        byte[] bBuff = new byte[1577];
        bBuff = Str2Byte(ref strBuff);

        head.SetDataB(MidB2B(ref bBuff, 1, 11));
        KettoNum = MidB2S(ref bBuff, 12, 10);
        DelKubun = MidB2S(ref bBuff, 22, 1);
        RegDate.SetDataB(MidB2B(ref bBuff, 23, 8));
        DelDate.SetDataB(MidB2B(ref bBuff, 31, 8));
        BirthDate.SetDataB(MidB2B(ref bBuff, 39, 8));
        Bamei = MidB2S(ref bBuff, 47, 36);
        BameiKana = MidB2S(ref bBuff, 83, 36);
        BameiEng = MidB2S(ref bBuff, 119, 60);
        ZaikyuFlag = MidB2S(ref bBuff, 179, 1);
        Reserved = MidB2S(ref bBuff, 180, 19);
        UmaKigoCD = MidB2S(ref bBuff, 199, 2);
        SexCD = MidB2S(ref bBuff, 201, 1);
        HinsyuCD = MidB2S(ref bBuff, 202, 1);
        KeiroCD = MidB2S(ref bBuff, 203, 2);

        for (i = 0; i < 14; i++)
        {
          Ketto3Info[i].SetDataB(MidB2B(ref bBuff, 205 + (44 * i), 44));
        }

        TozaiCD = MidB2S(ref bBuff, 821, 1);
        ChokyosiCode = MidB2S(ref bBuff, 822, 5);
        ChokyosiRyakusyo = MidB2S(ref bBuff, 827, 8);
        Syotai = MidB2S(ref bBuff, 835, 20);
        BreederCode = MidB2S(ref bBuff, 855, 6);
        BreederName = MidB2S(ref bBuff, 861, 70);
        SanchiName = MidB2S(ref bBuff, 931, 20);
        BanusiCode = MidB2S(ref bBuff, 951, 6);
        BanusiName = MidB2S(ref bBuff, 957, 64);
        RuikeiHonsyoHeiti = MidB2S(ref bBuff, 1021, 9);
        RuikeiHonsyoSyogai = MidB2S(ref bBuff, 1030, 9);
        RuikeiFukaHeichi = MidB2S(ref bBuff, 1039, 9);
        RuikeiFukaSyogai = MidB2S(ref bBuff, 1048, 9);
        RuikeiSyutokuHeichi = MidB2S(ref bBuff, 1057, 9);
        RuikeiSyutokuSyogai = MidB2S(ref bBuff, 1066, 9);
        ChakuSogo.SetDataB(MidB2B(ref bBuff, 1075, 18));
        ChakuChuo.SetDataB(MidB2B(ref bBuff, 1093, 18));

        for (i = 0; i < 7; i++)
        {
          ChakuKaisuBa[i].SetDataB(MidB2B(ref bBuff, 1111 + (18 * i), 18));
        }
        for (i = 0; i < 12; i++)
        {
          ChakuKaisuJyotai[i].SetDataB(MidB2B(ref bBuff, 1237 + (18 * i), 18));
        }
        for (i = 0; i < 6; i++)
        {
          ChakuKaisuKyori[i].SetDataB(MidB2B(ref bBuff, 1453 + (18 * i), 18));
        }
        for (i = 0; i < 4; i++)
        {
          Kyakusitu[i] = MidB2S(ref bBuff, 1561 + (3 * i), 3);
        }

        RaceCount = MidB2S(ref bBuff, 1573, 3);
        crlf = MidB2S(ref bBuff, 1576, 2);
        bBuff = null;
      }
    }

    #endregion

    #region 12.騎手マスタ

    /// <summary>
    /// 初騎乗情報
    /// </summary>
    public struct HATUKIJYO_INFO
    {
      public RACE_ID Hatukijyoid;     // 年月日場回日R
      public string SyussoTosu;       // 出走頭数
      public string KettoNum;         // 血統登録番号
      public string Bamei;            // 馬名

      public string KakuteiJyuni;     // 確定着順

      public string IJyoCD;           // 異常区分コード


      // データセット
      public void SetDataB(byte[] bBuff)
      {
        Hatukijyoid.SetDataB(MidB2B(ref bBuff, 1, 16));
        SyussoTosu = MidB2S(ref bBuff, 17, 2);
        KettoNum = MidB2S(ref bBuff, 19, 10);
        Bamei = MidB2S(ref bBuff, 29, 36);
        KakuteiJyuni = MidB2S(ref bBuff, 65, 2);
        IJyoCD = MidB2S(ref bBuff, 67, 1);
      }
    }

    /// <summary>
    /// 初勝利情報
    /// </summary>
    public struct HATUSYORI_INFO
    {
      public RACE_ID Hatukijyoid;     // 年月日場回日R
      public string SyussoTosu;       // 出走頭数
      public string KettoNum;         // 血統登録番号
      public string Bamei;            // 馬名


      // データセット
      public void SetDataB(byte[] bBuff)
      {
        Hatukijyoid.SetDataB(MidB2B(ref bBuff, 1, 16));
        SyussoTosu = MidB2S(ref bBuff, 17, 2);
        KettoNum = MidB2S(ref bBuff, 19, 10);
        Bamei = MidB2S(ref bBuff, 29, 36);
      }
    }

    public struct JV_KS_KISYU
    {
      public RECORD_ID head;                          // <レコードヘッダー>
      public string KisyuCode;                        // 騎手コード

      public string DelKubun;                         // 騎手抹消区分

      public YMD IssueDate;                           // 騎手免許交付年月日
      public YMD DelDate;                             // 騎手免許抹消年月日
      public YMD BirthDate;                           // 生年月日
      public string KisyuName;                        // 騎手名漢字

      public string reserved;                         // 予備
      public string KisyuNameKana;                    // 騎手名半角カナ

      public string KisyuRyakusyo;                    // 騎手名略称
      public string KisyuNameEng;                     // 騎手名欧字

      public string SexCD;                            // 性別区分

      public string SikakuCD;                         // 騎乗資格コード

      public string MinaraiCD;                        // 騎手見習コード

      public string TozaiCD;                          // 騎手東西所属コード

      public string Syotai;                           // 招待地域名
      public string ChokyosiCode;                     // 所属調教師コード

      public string ChokyosiRyakusyo;                 // 所属調教師名略称
      public HATUKIJYO_INFO[] HatuKiJyo;              // <初騎乗情報>
      public HATUSYORI_INFO[] HatuSyori;              // <初勝利情報>
      public SAIKIN_JYUSYO_INFO[] SaikinJyusyo;       // <最近重賞勝利情報>
      public HON_ZEN_RUIKEISEI_INFO[] HonZenRuikei;   // <本年・前年・累計成績情報>
      public string crlf;                             // レコード区切り

      // 配列の初期化

      public void Initialize()
      {
        HatuKiJyo = new HATUKIJYO_INFO[2];
        HatuSyori = new HATUSYORI_INFO[2];
        SaikinJyusyo = new SAIKIN_JYUSYO_INFO[3];
        HonZenRuikei = new HON_ZEN_RUIKEISEI_INFO[3];
      }

      // データセット
      public void SetDataB(ref string strBuff)
      {
        Initialize();
        int i;
        byte[] bBuff = new byte[4173];
        bBuff = Str2Byte(ref strBuff);

        head.SetDataB(MidB2B(ref bBuff, 1, 11));
        KisyuCode = MidB2S(ref bBuff, 12, 5);
        DelKubun = MidB2S(ref bBuff, 17, 1);
        IssueDate.SetDataB(MidB2B(ref bBuff, 18, 8));
        DelDate.SetDataB(MidB2B(ref bBuff, 26, 8));
        BirthDate.SetDataB(MidB2B(ref bBuff, 34, 8));
        KisyuName = MidB2S(ref bBuff, 42, 34);
        reserved = MidB2S(ref bBuff, 76, 34);
        KisyuNameKana = MidB2S(ref bBuff, 110, 30);
        KisyuRyakusyo = MidB2S(ref bBuff, 140, 8);
        KisyuNameEng = MidB2S(ref bBuff, 148, 80);
        SexCD = MidB2S(ref bBuff, 228, 1);
        SikakuCD = MidB2S(ref bBuff, 229, 1);
        MinaraiCD = MidB2S(ref bBuff, 230, 1);
        TozaiCD = MidB2S(ref bBuff, 231, 1);
        Syotai = MidB2S(ref bBuff, 232, 20);
        ChokyosiCode = MidB2S(ref bBuff, 252, 5);
        ChokyosiRyakusyo = MidB2S(ref bBuff, 257, 8);

        for (i = 0; i < 2; i++)
        {
          HatuKiJyo[i].SetDataB(MidB2B(ref bBuff, 265 + (67 * i), 67));
        }
        for (i = 0; i < 2; i++)
        {
          HatuSyori[i].SetDataB(MidB2B(ref bBuff, 399 + (64 * i), 64));
        }
        for (i = 0; i < 3; i++)
        {
          SaikinJyusyo[i].SetDataB(MidB2B(ref bBuff, 527 + (163 * i), 163));
        }
        for (i = 0; i < 3; i++)
        {
          HonZenRuikei[i].SetDataB(MidB2B(ref bBuff, 1016 + (1052 * i), 1052));
        }

        crlf = MidB2S(ref bBuff, 4172, 2);
        bBuff = null;
      }
    }

    #endregion

    #region 13.調教師マスタ

    public struct JV_CH_CHOKYOSI
    {
      public RECORD_ID head;                          // <レコードヘッダー>
      public string ChokyosiCode;                     // 調教師コード

      public string DelKubun;                         // 調教師抹消区分

      public YMD IssueDate;                           // 調教師免許交付年月日
      public YMD DelDate;                             // 調教師免許抹消年月日
      public YMD BirthDate;                           // 生年月日
      public string ChokyosiName;                     // 調教師名漢字

      public string ChokyosiNameKana;                 // 調教師名半角カナ

      public string ChokyosiRyakusyo;                 // 調教師名略称
      public string ChokyosiNameEng;                  // 調教師名欧字

      public string SexCD;                            // 性別区分

      public string TozaiCD;                          // 調教師東西所属コード

      public string Syotai;                           // 招待地域名
      public SAIKIN_JYUSYO_INFO[] SaikinJyusyo;       // <最近重賞勝利情報>
      public HON_ZEN_RUIKEISEI_INFO[] HonZenRuikei;   // <本年・前年・累計成績情報>
      public string crlf;                             // レコード区切り

      // 配列の初期化

      public void Initialize()
      {
        SaikinJyusyo = new SAIKIN_JYUSYO_INFO[3];
        HonZenRuikei = new HON_ZEN_RUIKEISEI_INFO[3];
      }

      // データセット
      public void SetDataB(ref string strBuff)
      {
        Initialize();
        int i;
        byte[] bBuff = new byte[3862];
        bBuff = Str2Byte(ref strBuff);

        head.SetDataB(MidB2B(ref bBuff, 1, 11));
        ChokyosiCode = MidB2S(ref bBuff, 12, 5);
        DelKubun = MidB2S(ref bBuff, 17, 1);
        IssueDate.SetDataB(MidB2B(ref bBuff, 18, 8));
        DelDate.SetDataB(MidB2B(ref bBuff, 26, 8));
        BirthDate.SetDataB(MidB2B(ref bBuff, 34, 8));
        ChokyosiName = MidB2S(ref bBuff, 42, 34);
        ChokyosiNameKana = MidB2S(ref bBuff, 76, 30);
        ChokyosiRyakusyo = MidB2S(ref bBuff, 106, 8);
        ChokyosiNameEng = MidB2S(ref bBuff, 114, 80);
        SexCD = MidB2S(ref bBuff, 194, 1);
        TozaiCD = MidB2S(ref bBuff, 195, 1);
        Syotai = MidB2S(ref bBuff, 196, 20);

        for (i = 0; i < 3; i++)
        {
          SaikinJyusyo[i].SetDataB(MidB2B(ref bBuff, 216 + (163 * i), 163));
        }
        for (i = 0; i < 3; i++)
        {
          HonZenRuikei[i].SetDataB(MidB2B(ref bBuff, 705 + (1052 * i), 1052));
        }

        crlf = MidB2S(ref bBuff, 3861, 2);
        bBuff = null;
      }
    }

    #endregion

    #region 14.生産者マスタ

    public struct JV_BR_BREEDER
    {
      public RECORD_ID head;                  // <レコードヘッダー>
      public string BreederCode;              // 生産者コード

      public string BreederName_Co;           // 生産者名(法人格有)
      public string BreederName;              // 生産者名(法人格無)
      public string BreederNameKana;          // 生産者名半角カナ

      public string BreederNameEng;           // 生産者名欧字

      public string Address;                  // 生産者住所自治省名
      public SEI_RUIKEI_INFO[] HonRuikei;     // <本年・累計成績情報>
      public string crlf;                     // レコード区切り

      // 配列の初期化

      public void Initialize()
      {
        HonRuikei = new SEI_RUIKEI_INFO[2];
      }

      // データセット
      public void SetDataB(ref string strBuff)
      {
        Initialize();
        byte[] bBuff = new byte[537];
        bBuff = Str2Byte(ref strBuff);

        head.SetDataB(MidB2B(ref bBuff, 1, 11));
        BreederCode = MidB2S(ref bBuff, 12, 6);
        BreederName_Co = MidB2S(ref bBuff, 18, 70);
        BreederName = MidB2S(ref bBuff, 88, 70);
        BreederNameKana = MidB2S(ref bBuff, 158, 70);
        BreederNameEng = MidB2S(ref bBuff, 228, 168);
        Address = MidB2S(ref bBuff, 396, 20);

        for (int i = 0; i < 2; i++)
        {
          HonRuikei[i].SetDataB(MidB2B(ref bBuff, 416 + (60 * i), 60));
        }

        crlf = MidB2S(ref bBuff, 536, 2);
        bBuff = null;
      }
    }

    #endregion

    #region 15.馬主マスタ

    public struct JV_BN_BANUSI
    {
      public RECORD_ID head;                  // <レコードヘッダー>
      public string BanusiCode;               // 馬主コード

      public string BanusiName_Co;            // 馬主名(法人格有)
      public string BanusiName;               // 馬主名(法人格無)
      public string BanusiNameKana;           // 馬主名半角カナ

      public string BanusiNameEng;            // 馬主名欧字

      public string Fukusyoku;                // 服色標示
      public SEI_RUIKEI_INFO[] HonRuikei;     // <本年・累計成績情報>
      public string crlf;                     // レコード区切り

      // 配列の初期化

      public void Initialize()
      {
        HonRuikei = new SEI_RUIKEI_INFO[2];
      }

      // データセット
      public void SetDataB(ref string strBuff)
      {
        Initialize();
        byte[] bBuff = new byte[477];
        bBuff = Str2Byte(ref strBuff);

        head.SetDataB(MidB2B(ref bBuff, 1, 11));
        BanusiCode = MidB2S(ref bBuff, 12, 6);
        BanusiName_Co = MidB2S(ref bBuff, 18, 64);
        BanusiName = MidB2S(ref bBuff, 82, 64);
        BanusiNameKana = MidB2S(ref bBuff, 146, 50);
        BanusiNameEng = MidB2S(ref bBuff, 196, 100);
        Fukusyoku = MidB2S(ref bBuff, 296, 60);

        for (int i = 0; i < 2; i++)
        {
          HonRuikei[i].SetDataB(MidB2B(ref bBuff, 356 + (60 * i), 60));
        }

        crlf = MidB2S(ref bBuff, 476, 2);
        bBuff = null;
      }
    }

    #endregion

    #region 16.繁殖馬マスタ

    public struct JV_HN_HANSYOKU
    {
      public RECORD_ID head;              // <レコードヘッダー>
      public string HansyokuNum;          // 繁殖登録番号
      public string reserved;             // 予備
      public string KettoNum;             // 血統登録番号
      public string DelKubun;             // 繁殖馬抹消区分(現在は予備として使用)
      public string Bamei;                // 馬名

      public string BameiKana;            // 馬名半角カナ

      public string BameiEng;             // 馬名欧字

      public string BirthYear;            // 生年
      public string SexCD;                // 性別コード

      public string HinsyuCD;             // 品種コード

      public string KeiroCD;              // 毛色コード

      public string HansyokuMochiKubun;   // 繁殖馬持込区分

      public string ImportYear;           // 輸入年
      public string SanchiName;           // 産地名

      public string HansyokuFNum;         // 父馬繁殖登録番号
      public string HansyokuMNum;         // 母馬繁殖登録番号
      public string crlf;                 // レコード区切り

      // データセット
      public void SetDataB(ref string strBuff)
      {
        byte[] bBuff = new byte[245];
        bBuff = Str2Byte(ref strBuff);

        head.SetDataB(MidB2B(ref bBuff, 1, 11));
        HansyokuNum = MidB2S(ref bBuff, 12, 8);
        reserved = MidB2S(ref bBuff, 20, 8);
        KettoNum = MidB2S(ref bBuff, 28, 10);
        DelKubun = MidB2S(ref bBuff, 38, 1);
        Bamei = MidB2S(ref bBuff, 39, 36);
        BameiKana = MidB2S(ref bBuff, 75, 40);
        BameiEng = MidB2S(ref bBuff, 115, 80);
        BirthYear = MidB2S(ref bBuff, 195, 4);
        SexCD = MidB2S(ref bBuff, 199, 1);
        HinsyuCD = MidB2S(ref bBuff, 200, 1);
        KeiroCD = MidB2S(ref bBuff, 201, 2);
        HansyokuMochiKubun = MidB2S(ref bBuff, 203, 1);
        ImportYear = MidB2S(ref bBuff, 204, 4);
        SanchiName = MidB2S(ref bBuff, 208, 20);
        HansyokuFNum = MidB2S(ref bBuff, 228, 8);
        HansyokuMNum = MidB2S(ref bBuff, 236, 8);
        crlf = MidB2S(ref bBuff, 244, 2);
        bBuff = null;
      }
    }

    #endregion

    #region 17.産駒マスタ

    public struct JV_SK_SANKU
    {
      public RECORD_ID head;          // <レコードヘッダー>
      public string KettoNum;         // 血統登録番号
      public YMD BirthDate;           // 生年月日
      public string SexCD;            // 性別コード

      public string HinsyuCD;         // 品種コード

      public string KeiroCD;          // 毛色コード

      public string SankuMochiKubun;  // 産駒持込区分

      public string ImportYear;       // 輸入年
      public string BreederCode;      // 生産者コード

      public string SanchiName;       // 産地名

      public string[] HansyokuNum;    // 3代血統 繁殖登録番号
      public string crlf;             // レコード区切り

      // 配列の初期化

      public void Initialize()
      {
        HansyokuNum = new string[14];
      }

      // データセット
      public void SetDataB(ref string strBuff)
      {
        Initialize();
        byte[] bBuff = new byte[178];
        bBuff = Str2Byte(ref strBuff);

        head.SetDataB(MidB2B(ref bBuff, 1, 11));
        KettoNum = MidB2S(ref bBuff, 12, 10);
        BirthDate.SetDataB(MidB2B(ref bBuff, 22, 8));
        SexCD = MidB2S(ref bBuff, 30, 1);
        HinsyuCD = MidB2S(ref bBuff, 31, 1);
        KeiroCD = MidB2S(ref bBuff, 32, 2);
        SankuMochiKubun = MidB2S(ref bBuff, 34, 1);
        ImportYear = MidB2S(ref bBuff, 35, 4);
        BreederCode = MidB2S(ref bBuff, 39, 6);
        SanchiName = MidB2S(ref bBuff, 45, 20);

        for (int i = 0; i < 14; i++)
        {
          HansyokuNum[i] = MidB2S(ref bBuff, 65 + (8 * i), 8);
        }

        crlf = MidB2S(ref bBuff, 177, 2);
        bBuff = null;
      }
    }

    #endregion

    #region 18.レコードマスタ

    /// <summary>
    /// レコード保持馬情報
    /// </summary>
    public struct RECUMA_INFO
    {
      public string KettoNum;         // 血統登録番号
      public string Bamei;            // 馬名

      public string UmaKigoCD;        // 馬記号コード

      public string SexCD;            // 性別コード

      public string ChokyosiCode;     // 調教師コード

      public string ChokyosiName;     // 調教師名

      public string Futan;            // 負担重量

      public string KisyuCode;        // 騎手コード

      public string KisyuName;        // 騎手名


      // データセット
      public void SetDataB(byte[] bBuff)
      {
        KettoNum = MidB2S(ref bBuff, 1, 10);
        Bamei = MidB2S(ref bBuff, 11, 36);
        UmaKigoCD = MidB2S(ref bBuff, 47, 2);
        SexCD = MidB2S(ref bBuff, 49, 1);
        ChokyosiCode = MidB2S(ref bBuff, 50, 5);
        ChokyosiName = MidB2S(ref bBuff, 55, 34);
        Futan = MidB2S(ref bBuff, 89, 3);
        KisyuCode = MidB2S(ref bBuff, 92, 5);
        KisyuName = MidB2S(ref bBuff, 97, 34);
      }
    }

    public struct JV_RC_RECORD
    {
      public RECORD_ID head;              // <レコードヘッダー>
      public string RecInfoKubun;         // レコード識別区分

      public RACE_ID id;                  // <競走識別情報>
      public string TokuNum;              // 特別競走番号
      public string Hondai;               // 競走名本題

      public string GradeCD;              // グレードコード

      public string SyubetuCD;            // 競走種別コード

      public string Kyori;                // 距離
      public string TrackCD;              // トラックコード

      public string RecKubun;             // レコード区分

      public string RecTime;              // レコードタイム
      public TENKO_BABA_INFO TenkoBaba;   // 天候・馬場状態

      public RECUMA_INFO[] RecUmaInfo;    // <レコード保持馬情報>
      public string crlf;                 // レコード区切り

      // 配列の初期化

      public void Initialize()
      {
        RecUmaInfo = new RECUMA_INFO[3];
      }

      // データセット
      public void SetDataB(ref string strBuff)
      {
        Initialize();
        byte[] bBuff = new byte[501];
        bBuff = Str2Byte(ref strBuff);

        head.SetDataB(MidB2B(ref bBuff, 1, 11));
        RecInfoKubun = MidB2S(ref bBuff, 12, 1);
        id.SetDataB(MidB2B(ref bBuff, 13, 16));
        TokuNum = MidB2S(ref bBuff, 29, 4);
        Hondai = MidB2S(ref bBuff, 33, 60);
        GradeCD = MidB2S(ref bBuff, 93, 1);
        SyubetuCD = MidB2S(ref bBuff, 94, 2);
        Kyori = MidB2S(ref bBuff, 96, 4);
        TrackCD = MidB2S(ref bBuff, 100, 2);
        RecKubun = MidB2S(ref bBuff, 102, 1);
        RecTime = MidB2S(ref bBuff, 103, 4);
        TenkoBaba.SetDataB(MidB2B(ref bBuff, 107, 3));

        for (int i = 0; i < 3; i++)
        {
          RecUmaInfo[i].SetDataB(MidB2B(ref bBuff, 110 + (130 * i), 130));
        }

        crlf = MidB2S(ref bBuff, 500, 2);
        bBuff = null;
      }
    }

    #endregion

    #region 19.坂路調教


    public struct JV_HC_HANRO
    {
      public RECORD_ID head;      // <レコードヘッダー>
      public string TresenKubun;  // トレセン区分

      public YMD ChokyoDate;      // 調教年月日
      public string ChokyoTime;   // 調教時刻
      public string KettoNum;     // 血統登録番号
      public string HaronTime4;   // 4ハロンタイム合計(800M-0M)
      public string LapTime4;     // ラップタイム(800M-600M)
      public string HaronTime3;   // 3ハロンタイム合計(600M-0M)
      public string LapTime3;     // ラップタイム(600M-400M)
      public string HaronTime2;   // 2ハロンタイム合計(400M-0M)
      public string LapTime2;     // ラップタイム(400M-200M)
      public string LapTime1;     // ラップタイム(200M-0M)
      public string crlf;         // レコード区切り

      // データセット
      public void SetDataB(ref string strBuff)
      {
        byte[] bBuff = new byte[60];
        bBuff = Str2Byte(ref strBuff);

        head.SetDataB(MidB2B(ref bBuff, 1, 11));
        TresenKubun = MidB2S(ref bBuff, 12, 1);
        ChokyoDate.SetDataB(MidB2B(ref bBuff, 13, 8));
        ChokyoTime = MidB2S(ref bBuff, 21, 4);
        KettoNum = MidB2S(ref bBuff, 25, 10);
        HaronTime4 = MidB2S(ref bBuff, 35, 4);
        LapTime4 = MidB2S(ref bBuff, 39, 3);
        HaronTime3 = MidB2S(ref bBuff, 42, 4);
        LapTime3 = MidB2S(ref bBuff, 46, 3);
        HaronTime2 = MidB2S(ref bBuff, 49, 4);
        LapTime2 = MidB2S(ref bBuff, 53, 3);
        LapTime1 = MidB2S(ref bBuff, 56, 3);
        crlf = MidB2S(ref bBuff, 59, 2);
        bBuff = null;
      }
    }

    #endregion

    #region 20.馬体重

    /// <summary>
    /// 馬体重情報
    /// </summary>
    public struct BATAIJYU_INFO
    {
      public string Umaban;       // 馬番
      public string Bamei;        // 馬名

      public string BaTaijyu;     // 馬体重
      public string ZogenFugo;    // 増減符号
      public string ZogenSa;      // 増減差

      // データセット
      public void SetDataB(byte[] bBuff)
      {
        Umaban = MidB2S(ref bBuff, 1, 2);
        Bamei = MidB2S(ref bBuff, 3, 36);
        BaTaijyu = MidB2S(ref bBuff, 39, 3);
        ZogenFugo = MidB2S(ref bBuff, 42, 1);
        ZogenSa = MidB2S(ref bBuff, 43, 3);
      }
    }

    public struct JV_WH_BATAIJYU
    {
      public RECORD_ID head;                  // <レコードヘッダー>
      public RACE_ID id;                      // <競走識別情報>
      public MDHM HappyoTime;                 // 発表月日時分
      public BATAIJYU_INFO[] BataijyuInfo;    // <馬体重情報>
      public string crlf;                     // レコード区切り

      // 配列の初期化

      public void Initialize()
      {
        BataijyuInfo = new BATAIJYU_INFO[18];
      }

      // データセット
      public void SetDataB(ref string strBuff)
      {
        Initialize();
        byte[] bBuff = new byte[847];
        bBuff = Str2Byte(ref strBuff);

        head.SetDataB(MidB2B(ref bBuff, 1, 11));
        id.SetDataB(MidB2B(ref bBuff, 12, 16));
        HappyoTime.SetDataB(MidB2B(ref bBuff, 28, 8));

        for (int i = 0; i < 18; i++)
        {
          BataijyuInfo[i].SetDataB(MidB2B(ref bBuff, 36 + (45 * i), 45));
        }

        crlf = MidB2S(ref bBuff, 846, 2);
        bBuff = null;
      }
    }

    #endregion

    #region 21.天候馬場状態


    public struct JV_WE_WEATHER
    {
      public RECORD_ID head;                      // <レコードヘッダー>
      public RACE_ID2 id;                         // <競走識別情報２>
      public MDHM HappyoTime;                     // 発表月日時分
      public string HenkoID;                      // 変更識別
      public TENKO_BABA_INFO TenkoBaba;           // 現在状態情報
      public TENKO_BABA_INFO TenkoBabaBefore;     // 変更前状態情報
      public string crlf;                         // レコード区切り

      // データセット
      public void SetDataB(ref string strBuff)
      {
        byte[] bBuff = new byte[42];
        bBuff = Str2Byte(ref strBuff);

        head.SetDataB(MidB2B(ref bBuff, 1, 11));
        id.SetDataB(MidB2B(ref bBuff, 12, 14));
        HappyoTime.SetDataB(MidB2B(ref bBuff, 26, 8));
        HenkoID = MidB2S(ref bBuff, 34, 1);
        TenkoBaba.SetDataB(MidB2B(ref bBuff, 35, 3));
        TenkoBabaBefore.SetDataB(MidB2B(ref bBuff, 38, 3));
        crlf = MidB2S(ref bBuff, 41, 2);
        bBuff = null;
      }
    }

    #endregion

    #region 22.出走取消・競走除外


    public struct JV_AV_INFO
    {
      public RECORD_ID head;      // <レコードヘッダー>
      public RACE_ID id;          // <競走識別情報>
      public MDHM HappyoTime;     // 発表月日時分
      public string Umaban;       // 馬番
      public string Bamei;        // 馬名

      public string JiyuKubun;    // 事由区分

      public string crlf;         // レコード区切り

      // データセット
      public void SetDataB(ref string strBuff)
      {
        byte[] bBuff = new byte[78];
        bBuff = Str2Byte(ref strBuff);

        head.SetDataB(MidB2B(ref bBuff, 1, 11));
        id.SetDataB(MidB2B(ref bBuff, 12, 16));
        HappyoTime.SetDataB(MidB2B(ref bBuff, 28, 8));
        Umaban = MidB2S(ref bBuff, 36, 2);
        Bamei = MidB2S(ref bBuff, 38, 36);
        JiyuKubun = MidB2S(ref bBuff, 74, 3);
        crlf = MidB2S(ref bBuff, 77, 2);
        bBuff = null;
      }
    }

    #endregion

    #region 23.騎手変更

    /// <summary>
    /// 変更情報
    /// </summary>
    public struct JC_INFO
    {
      public string Futan;        // 負担重量

      public string KisyuCode;    // 騎手コード

      public string KisyuName;    // 騎手名

      public string MinaraiCD;    // 騎手見習コード


      // データセット
      public void SetDataB(byte[] bBuff)
      {
        Futan = MidB2S(ref bBuff, 1, 3);
        KisyuCode = MidB2S(ref bBuff, 4, 5);
        KisyuName = MidB2S(ref bBuff, 9, 34);
        MinaraiCD = MidB2S(ref bBuff, 43, 1);
      }
    }

    public struct JV_JC_INFO
    {
      public RECORD_ID head;          // <レコードヘッダー>
      public RACE_ID id;              // <競走識別情報>
      public MDHM HappyoTime;         // 発表月日時分
      public string Umaban;           // 馬番
      public string Bamei;            // 馬名

      public JC_INFO JCInfoAfter;     // <変更後情報>
      public JC_INFO JCInfoBefore;    // <変更前情報>
      public string crlf;             // レコード区切り

      // データセット
      public void SetDataB(ref string strBuff)
      {
        byte[] bBuff = new byte[161];
        bBuff = Str2Byte(ref strBuff);

        head.SetDataB(MidB2B(ref bBuff, 1, 11));
        id.SetDataB(MidB2B(ref bBuff, 12, 16));
        HappyoTime.SetDataB(MidB2B(ref bBuff, 28, 8));
        Umaban = MidB2S(ref bBuff, 36, 2);
        Bamei = MidB2S(ref bBuff, 38, 36);
        JCInfoAfter.SetDataB(MidB2B(ref bBuff, 74, 43));
        JCInfoBefore.SetDataB(MidB2B(ref bBuff, 117, 43));
        crlf = MidB2S(ref bBuff, 160, 2);
        bBuff = null;
      }
    }

    #endregion

    #region 23-1.発走時刻変更

    /// <summary>
    /// 変更情報
    /// </summary>
    public struct TC_INFO
    {
      public string Ji;   // 時

      public string Fun;  // 分


      // データセット
      public void SetDataB(byte[] bBuff)
      {
        Ji = MidB2S(ref bBuff, 1, 2);
        Fun = MidB2S(ref bBuff, 3, 2);
      }
    }

    public struct JV_TC_INFO
    {
      public RECORD_ID head;          // <レコードヘッダー>
      public RACE_ID id;              // <競走識別情報>
      public MDHM HappyoTime;         // 発表月日時分
      public TC_INFO TCInfoAfter;     // <変更後情報>
      public TC_INFO TCInfoBefore;    // <変更前情報>
      public string crlf;             // レコード区切り

      // データセット
      public void SetDataB(ref string strBuff)
      {
        byte[] bBuff = new byte[45];
        bBuff = Str2Byte(ref strBuff);

        head.SetDataB(MidB2B(ref bBuff, 1, 11));
        id.SetDataB(MidB2B(ref bBuff, 12, 16));
        HappyoTime.SetDataB(MidB2B(ref bBuff, 28, 8));
        TCInfoAfter.SetDataB(MidB2B(ref bBuff, 36, 4));
        TCInfoBefore.SetDataB(MidB2B(ref bBuff, 40, 4));
        crlf = MidB2S(ref bBuff, 44, 2);
        bBuff = null;
      }
    }

    #endregion

    #region 23-2.コース変更

    /// <summary>
    /// 変更情報
    /// </summary>
    public struct CC_INFO
    {
      public string Kyori;    // 距離
      public string TruckCd;  // トラックコード


      // データセット
      public void SetDataB(byte[] bBuff)
      {
        Kyori = MidB2S(ref bBuff, 1, 4);
        TruckCd = MidB2S(ref bBuff, 5, 2);
      }
    }

    public struct JV_CC_INFO
    {
      public RECORD_ID head;          // <レコードヘッダー>
      public RACE_ID id;              // <競走識別情報>
      public MDHM HappyoTime;         // 発表月日時分
      public CC_INFO CCInfoAfter;     // <変更後情報>
      public CC_INFO CCInfoBefore;    // <変更前情報>
      public string JiyuCd;
      public string crlf;             // レコード区切り

      // データセット
      public void SetDataB(ref string strBuff)
      {
        byte[] bBuff = new byte[50];
        bBuff = Str2Byte(ref strBuff);

        head.SetDataB(MidB2B(ref bBuff, 1, 11));
        id.SetDataB(MidB2B(ref bBuff, 12, 16));
        HappyoTime.SetDataB(MidB2B(ref bBuff, 28, 8));
        CCInfoAfter.SetDataB(MidB2B(ref bBuff, 36, 6));
        CCInfoBefore.SetDataB(MidB2B(ref bBuff, 42, 6));
        JiyuCd = MidB2S(ref bBuff, 48, 1);
        crlf = MidB2S(ref bBuff, 49, 2);
        bBuff = null;
      }
    }

    #endregion

    #region 24.データマイニング予想

    /// <summary>
    /// マイニング予想
    /// </summary>
    public struct DM_INFO
    {
      public string Umaban;     // 馬番
      public string DMTime;     // 予想走破タイム
      public string DMGosaP;    // 予想誤差(信頼度)＋

      public string DMGosaM;    // 予想誤差(信頼度)－


      // データセット
      public void SetDataB(byte[] bBuff)
      {
        Umaban = MidB2S(ref bBuff, 1, 2);
        DMTime = MidB2S(ref bBuff, 3, 5);
        DMGosaP = MidB2S(ref bBuff, 8, 4);
        DMGosaM = MidB2S(ref bBuff, 12, 4);
      }
    }

    public struct JV_DM_INFO
    {
      public RECORD_ID head;      // <レコードヘッダー>
      public RACE_ID id;          // <競走識別情報>
      public HM MakeHM;           // データ作成時分
      public DM_INFO[] DMInfo;    // <マイニング予想>
      public string crlf;         // レコード区切り

      // 配列の初期化

      public void Initialize()
      {
        DMInfo = new DM_INFO[18];
      }

      // データセット
      public void SetDataB(ref string strBuff)
      {
        Initialize();
        byte[] bBuff = new byte[303];
        bBuff = Str2Byte(ref strBuff);

        head.SetDataB(MidB2B(ref bBuff, 1, 11));
        id.SetDataB(MidB2B(ref bBuff, 12, 16));
        MakeHM.SetDataB(MidB2B(ref bBuff, 28, 4));

        for (int i = 0; i < 18; i++)
        {
          DMInfo[i].SetDataB(MidB2B(ref bBuff, 32 + (15 * i), 15));
        }

        crlf = MidB2S(ref bBuff, 302, 2);
        bBuff = null;
      }
    }

    #endregion

    #region 25.開催スケジュール

    /// <summary>
    /// 重賞案内
    /// </summary>
    public struct JYUSYO_INFO
    {
      public string TokuNum;      // 特別競走番号
      public string Hondai;       // 競走名本題

      public string Ryakusyo10;   // 競走名略称10字

      public string Ryakusyo6;    // 競走名略称6字

      public string Ryakusyo3;    // 競走名略称3字

      public string Nkai;         // 重賞回次[第N回]
      public string GradeCD;      // グレードコード

      public string SyubetuCD;    // 競走種別コード

      public string KigoCD;       // 競走記号コード

      public string JyuryoCD;     // 重量種別コード

      public string Kyori;        // 距離
      public string TrackCD;      // トラックコード


      // データセット
      public void SetDataB(byte[] bBuff)
      {
        TokuNum = MidB2S(ref bBuff, 1, 4);
        Hondai = MidB2S(ref bBuff, 5, 60);
        Ryakusyo10 = MidB2S(ref bBuff, 65, 20);
        Ryakusyo6 = MidB2S(ref bBuff, 85, 12);
        Ryakusyo3 = MidB2S(ref bBuff, 97, 6);
        Nkai = MidB2S(ref bBuff, 103, 3);
        GradeCD = MidB2S(ref bBuff, 106, 1);
        SyubetuCD = MidB2S(ref bBuff, 107, 2);
        KigoCD = MidB2S(ref bBuff, 109, 3);
        JyuryoCD = MidB2S(ref bBuff, 112, 1);
        Kyori = MidB2S(ref bBuff, 113, 4);
        TrackCD = MidB2S(ref bBuff, 117, 2);
      }
    }

    public struct JV_YS_SCHEDULE
    {
      public RECORD_ID head;              // <レコードヘッダー>
      public RACE_ID2 id;                 // <競走識別情報２>
      public string YoubiCD;              // 曜日コード

      public JYUSYO_INFO[] JyusyoInfo;    // <重賞案内>
      public string crlf;                 // レコード区切り

      // 配列の初期化

      public void Initialize()
      {
        JyusyoInfo = new JYUSYO_INFO[3];
      }

      // データセット
      public void SetDataB(ref string strBuff)
      {
        Initialize();
        byte[] bBuff = new byte[382];
        bBuff = Str2Byte(ref strBuff);

        head.SetDataB(MidB2B(ref bBuff, 1, 11));
        id.SetDataB(MidB2B(ref bBuff, 12, 14));
        YoubiCD = MidB2S(ref bBuff, 26, 1);

        for (int i = 0; i < 3; i++)
        {
          JyusyoInfo[i].SetDataB(MidB2B(ref bBuff, 27 + (118 * i), 118));
        }

        crlf = MidB2S(ref bBuff, 381, 2);
        bBuff = null;
      }
    }

    #endregion

    #region 26.競走馬市場取引価格

    public struct JV_HS_SALE
    {
      public RECORD_ID head;         // <レコードヘッダー>
      public string KettoNum;        // 血統登録番号
      public string HansyokuFNum;    // 父馬繁殖登録番号
      public string HansyokuMNum;    // 母馬繁殖登録番号
      public string BirthYear;       // 生年
      public string SaleCode;        // 主催者・市場コード

      public string SaleHostName;    // 主催者名称
      public string SaleName;        // 市場の名称
      public YMD FromDate;           // 市場の開催期間(開始日)
      public YMD ToDate;             // 市場の開催期間(終了日)
      public string Barei;           // 取引時の競走馬の年齢
      public string Price;           // 取引価格
      public string crlf;            // レコード区切り

      // データセット
      public void SetDataB(ref string strBuff)
      {
        byte[] bBuff = new byte[196];
        bBuff = Str2Byte(ref strBuff);

        head.SetDataB(MidB2B(ref bBuff, 1, 11));
        KettoNum = MidB2S(ref bBuff, 12, 10);
        HansyokuFNum = MidB2S(ref bBuff, 22, 8);
        HansyokuMNum = MidB2S(ref bBuff, 30, 8);
        BirthYear = MidB2S(ref bBuff, 38, 4);
        SaleCode = MidB2S(ref bBuff, 42, 6);
        SaleHostName = MidB2S(ref bBuff, 48, 40);
        SaleName = MidB2S(ref bBuff, 88, 80);
        FromDate.SetDataB(MidB2B(ref bBuff, 168, 8));
        ToDate.SetDataB(MidB2B(ref bBuff, 176, 8));
        Barei = MidB2S(ref bBuff, 184, 1);
        Price = MidB2S(ref bBuff, 185, 10);
        crlf = MidB2S(ref bBuff, 195, 2);
        bBuff = null;
      }
    }

    #endregion

    #region 27.馬名の意味由来

    public struct JV_HY_BAMEIORIGIN
    {
      public RECORD_ID head;       // <レコードヘッダー>
      public string KettoNum;      // 血統登録番号
      public string Bamei;         // 馬名

      public string Origin;        // 馬名の意味由来
      public string crlf;          // レコード区切り

      // データセット
      public void SetDataB(ref string strBuff)
      {
        byte[] bBuff = new byte[123];
        bBuff = Str2Byte(ref strBuff);

        head.SetDataB(MidB2B(ref bBuff, 1, 11));
        KettoNum = MidB2S(ref bBuff, 12, 10);
        Bamei = MidB2S(ref bBuff, 22, 36);
        Origin = MidB2S(ref bBuff, 58, 64);
        crlf = MidB2S(ref bBuff, 122, 2);
        bBuff = null;
      }
    }

    #endregion

    #region 28.出走別着度数

    /// <summary>
    /// 出走別着度数 競走馬情報
    /// </summary>
    public struct JV_CK_UMA
    {
      public string KettoNum;                         // 血統登録番号
      public string Bamei;                            // 馬名

      public string RuikeiHonsyoHeiti;                // 平地本賞金累計

      public string RuikeiHonsyoSyogai;               // 障害本賞金累計

      public string RuikeiFukaHeichi;                 // 平地付加賞金累計

      public string RuikeiFukaSyogai;                 // 障害付加賞金累計

      public string RuikeiSyutokuHeichi;              // 平地収得賞金累計

      public string RuikeiSyutokuSyogai;              // 障害収得賞金累計

      public CHAKUKAISU3_INFO ChakuSogo;              // 総合着回数
      public CHAKUKAISU3_INFO ChakuChuo;              // 中央合計着回数
      public CHAKUKAISU3_INFO[] ChakuKaisuBa;         // 馬場別着回数
      public CHAKUKAISU3_INFO[] ChakuKaisuJyotai;     // 馬場状態別着回数
      public CHAKUKAISU3_INFO[] ChakuKaisuSibaKyori;  // 芝距離別着回数
      public CHAKUKAISU3_INFO[] ChakuKaisuDirtKyori;  // ダート距離別着回数
      public CHAKUKAISU3_INFO[] ChakuKaisuJyoSiba;    // 競馬場別芝着回数
      public CHAKUKAISU3_INFO[] ChakuKaisuJyoDirt;    // 競馬場別ダート着回数
      public CHAKUKAISU3_INFO[] ChakuKaisuJyoSyogai;  // 競馬場別障害着回数
      public string[] Kyakusitu;                      // 脚質傾向

      public string RaceCount;                        // 登録レース数

      // 配列の初期化

      public void Initialize()
      {
        ChakuKaisuBa = new CHAKUKAISU3_INFO[7];
        ChakuKaisuJyotai = new CHAKUKAISU3_INFO[12];
        ChakuKaisuSibaKyori = new CHAKUKAISU3_INFO[9];
        ChakuKaisuDirtKyori = new CHAKUKAISU3_INFO[9];
        ChakuKaisuJyoSiba = new CHAKUKAISU3_INFO[10];
        ChakuKaisuJyoDirt = new CHAKUKAISU3_INFO[10];
        ChakuKaisuJyoSyogai = new CHAKUKAISU3_INFO[10];
        Kyakusitu = new string[4];
      }

      // データセット
      public void SetDataB(byte[] bBuff)
      {
        Initialize();
        int i;
        KettoNum = MidB2S(ref bBuff, 1, 10);
        Bamei = MidB2S(ref bBuff, 11, 36);
        RuikeiHonsyoHeiti = MidB2S(ref bBuff, 47, 9);
        RuikeiHonsyoSyogai = MidB2S(ref bBuff, 56, 9);
        RuikeiFukaHeichi = MidB2S(ref bBuff, 65, 9);
        RuikeiFukaSyogai = MidB2S(ref bBuff, 74, 9);
        RuikeiSyutokuHeichi = MidB2S(ref bBuff, 83, 9);
        RuikeiSyutokuSyogai = MidB2S(ref bBuff, 92, 9);
        ChakuSogo.SetDataB(MidB2B(ref bBuff, 101, 18));
        ChakuChuo.SetDataB(MidB2B(ref bBuff, 119, 18));

        for (i = 0; i < 7; i++)
        {
          ChakuKaisuBa[i].SetDataB(MidB2B(ref bBuff, 137 + (18 * i), 18));
        }
        for (i = 0; i < 12; i++)
        {
          ChakuKaisuJyotai[i].SetDataB(MidB2B(ref bBuff, 263 + (18 * i), 18));
        }
        for (i = 0; i < 9; i++)
        {
          ChakuKaisuSibaKyori[i].SetDataB(MidB2B(ref bBuff, 479 + (18 * i), 18));
        }
        for (i = 0; i < 9; i++)
        {
          ChakuKaisuDirtKyori[i].SetDataB(MidB2B(ref bBuff, 641 + (18 * i), 18));
        }
        for (i = 0; i < 10; i++)
        {
          ChakuKaisuJyoSiba[i].SetDataB(MidB2B(ref bBuff, 803 + (18 * i), 18));
        }
        for (i = 0; i < 10; i++)
        {
          ChakuKaisuJyoDirt[i].SetDataB(MidB2B(ref bBuff, 983 + (18 * i), 18));
        }
        for (i = 0; i < 10; i++)
        {
          ChakuKaisuJyoSyogai[i].SetDataB(MidB2B(ref bBuff, 1163 + (18 * i), 18));
        }
        for (i = 0; i < 4; i++)
        {
          Kyakusitu[i] = MidB2S(ref bBuff, 1343 + (3 * i), 3);
        }

        RaceCount = MidB2S(ref bBuff, 1355, 3);
      }
    }

    /// <summary>
    /// 出走別着度数 本年・累計成績情報
    /// </summary>
    public struct JV_CK_HON_RUIKEISEI_INFO
    {
      public string SetYear;                          // 設定年
      public string HonSyokinHeichi;                  // 平地本賞金合計

      public string HonSyokinSyogai;                  // 障害本賞金合計

      public string FukaSyokinHeichi;                 // 平地付加賞金合計

      public string FukaSyokinSyogai;                 // 障害付加賞金合計

      public CHAKUKAISU5_INFO ChakuKaisuSiba;         // 芝着回数
      public CHAKUKAISU5_INFO ChakuKaisuDirt;         // ダート着回数
      public CHAKUKAISU4_INFO ChakuKaisuSyogai;       // 障害着回数
      public CHAKUKAISU4_INFO[] ChakuKaisuSibaKyori;  // 芝距離別着回数
      public CHAKUKAISU4_INFO[] ChakuKaisuDirtKyori;  // ダート距離別着回数
      public CHAKUKAISU4_INFO[] ChakuKaisuJyoSiba;    // 競馬場別芝着回数
      public CHAKUKAISU4_INFO[] ChakuKaisuJyoDirt;    // 競馬場別ダート着回数
      public CHAKUKAISU3_INFO[] ChakuKaisuJyoSyogai;  // 競馬場別障害着回数

      // 配列の初期化

      public void Initialize()
      {
        ChakuKaisuSibaKyori = new CHAKUKAISU4_INFO[9];
        ChakuKaisuDirtKyori = new CHAKUKAISU4_INFO[9];
        ChakuKaisuJyoSiba = new CHAKUKAISU4_INFO[10];
        ChakuKaisuJyoDirt = new CHAKUKAISU4_INFO[10];
        ChakuKaisuJyoSyogai = new CHAKUKAISU3_INFO[10];
      }

      // データセット
      public void SetDataB(byte[] bBuff)
      {
        Initialize();
        int i;
        SetYear = MidB2S(ref bBuff, 1, 4);
        HonSyokinHeichi = MidB2S(ref bBuff, 5, 10);
        HonSyokinSyogai = MidB2S(ref bBuff, 15, 10);
        FukaSyokinHeichi = MidB2S(ref bBuff, 25, 10);
        FukaSyokinSyogai = MidB2S(ref bBuff, 35, 10);
        ChakuKaisuSiba.SetDataB(MidB2B(ref bBuff, 45, 30));
        ChakuKaisuDirt.SetDataB(MidB2B(ref bBuff, 75, 30));
        ChakuKaisuSyogai.SetDataB(MidB2B(ref bBuff, 105, 24));

        for (i = 0; i < 9; i++)
        {
          ChakuKaisuSibaKyori[i].SetDataB(MidB2B(ref bBuff, 129 + (24 * i), 24));
        }
        for (i = 0; i < 9; i++)
        {
          ChakuKaisuDirtKyori[i].SetDataB(MidB2B(ref bBuff, 345 + (24 * i), 24));
        }
        for (i = 0; i < 10; i++)
        {
          ChakuKaisuJyoSiba[i].SetDataB(MidB2B(ref bBuff, 561 + (24 * i), 24));
        }
        for (i = 0; i < 10; i++)
        {
          ChakuKaisuJyoDirt[i].SetDataB(MidB2B(ref bBuff, 801 + (24 * i), 24));
        }
        for (i = 0; i < 10; i++)
        {
          ChakuKaisuJyoSyogai[i].SetDataB(MidB2B(ref bBuff, 1041 + (18 * i), 18));
        }
      }
    }

    /// <summary>
    /// 出走別着度数 騎手情報
    /// </summary>
    public struct JV_CK_KISYU
    {
      public string KisyuCode;                        // 騎手コード

      public string KisyuName;                        // 騎手名漢字

      public JV_CK_HON_RUIKEISEI_INFO[] HonRuikei;    // <本年・累計成績情報>

      // 配列の初期化

      public void Initialize()
      {
        HonRuikei = new JV_CK_HON_RUIKEISEI_INFO[2];
      }

      // データセット
      public void SetDataB(byte[] bBuff)
      {
        Initialize();
        KisyuCode = MidB2S(ref bBuff, 1, 5);
        KisyuName = MidB2S(ref bBuff, 6, 34);

        for (int i = 0; i < 2; i++)
        {
          HonRuikei[i].SetDataB(MidB2B(ref bBuff, 40 + (1220 * i), 1220));
        }
      }
    }

    /// <summary>
    /// 出走別着度数 調教師情報
    /// </summary>
    public struct JV_CK_CHOKYOSI
    {
      public string ChokyosiCode;                     // 調教師コード

      public string ChokyosiName;                     // 調教師名漢字

      public JV_CK_HON_RUIKEISEI_INFO[] HonRuikei;    // <本年・累計成績情報>

      // 配列の初期化

      public void Initialize()
      {
        HonRuikei = new JV_CK_HON_RUIKEISEI_INFO[2];
      }

      // データセット
      public void SetDataB(byte[] bBuff)
      {
        Initialize();
        ChokyosiCode = MidB2S(ref bBuff, 1, 5);
        ChokyosiName = MidB2S(ref bBuff, 6, 34);

        for (int i = 0; i < 2; i++)
        {
          HonRuikei[i].SetDataB(MidB2B(ref bBuff, 40 + (1220 * i), 1220));
        }
      }
    }

    /// <summary>
    /// 出走別着度数 馬主情報
    /// </summary>
    public struct JV_CK_BANUSI
    {
      public string BanusiCode;               // 馬主コード

      public string BanusiName_Co;            // 馬主名（法人格有）

      public string BanusiName;               // 馬主名（法人格無）

      public SEI_RUIKEI_INFO[] HonRuikei;     // <本年・累計成績情報>

      // 配列の初期化

      public void Initialize()
      {
        HonRuikei = new SEI_RUIKEI_INFO[2];
      }

      // データセット
      public void SetDataB(byte[] bBuff)
      {
        Initialize();
        BanusiCode = MidB2S(ref bBuff, 1, 6);
        BanusiName_Co = MidB2S(ref bBuff, 7, 64);
        BanusiName = MidB2S(ref bBuff, 71, 64);

        for (int i = 0; i < 2; i++)
        {
          HonRuikei[i].SetDataB(MidB2B(ref bBuff, 135 + (60 * i), 60));
        }
      }
    }

    /// <summary>
    /// 出走別着度数 生産者情報
    /// </summary>
    public struct JV_CK_BREEDER
    {
      public string BreederCode;              // 生産者コード

      public string BreederName_Co;           // 生産者（法人格有）

      public string BreederName;              // 生産者（法人格無）

      public SEI_RUIKEI_INFO[] HonRuikei;     // <本年・累計成績情報>

      // 配列の初期化

      public void Initialize()
      {
        HonRuikei = new SEI_RUIKEI_INFO[2];
      }

      // データセット
      public void SetDataB(byte[] bBuff)
      {
        Initialize();
        BreederCode = MidB2S(ref bBuff, 1, 6);
        BreederName_Co = MidB2S(ref bBuff, 7, 70);
        BreederName = MidB2S(ref bBuff, 77, 70);

        for (int i = 0; i < 2; i++)
        {
          HonRuikei[i].SetDataB(MidB2B(ref bBuff, 147 + (60 * i), 60));
        }
      }
    }

    public struct JV_CK_CHAKU
    {
      public RECORD_ID head;                   // <レコードヘッダー>
      public RACE_ID id;                       // <競走識別情報１>
      public JV_CK_UMA UmaChaku;               // <出走別着度数 競走馬情報>
      public JV_CK_KISYU KisyuChaku;           // <出走別着度数 騎手情報>
      public JV_CK_CHOKYOSI ChokyoChaku;       // <出走別着度数 調教師情報>
      public JV_CK_BANUSI BanusiChaku;         // <出走別着度数 馬主情報>
      public JV_CK_BREEDER BreederChaku;       // <出走別着度数 生産者情報>
      public string crlf;                      // レコード区切り

      // データセット
      public void SetDataB(ref string strBuff)
      {
        byte[] bBuff = new byte[6864];
        bBuff = Str2Byte(ref strBuff);

        head.SetDataB(MidB2B(ref bBuff, 1, 11));
        id.SetDataB(MidB2B(ref bBuff, 12, 16));
        UmaChaku.SetDataB(MidB2B(ref bBuff, 28, 1357));
        KisyuChaku.SetDataB(MidB2B(ref bBuff, 1385, 2479));
        ChokyoChaku.SetDataB(MidB2B(ref bBuff, 3864, 2479));
        BanusiChaku.SetDataB(MidB2B(ref bBuff, 6343, 254));
        BreederChaku.SetDataB(MidB2B(ref bBuff, 6597, 266));
        crlf = MidB2S(ref bBuff, 6863, 2);
        bBuff = null;
      }
    }

    #endregion

    #region 29.系統情報

    public struct JV_BT_KEITO
    {
      public RECORD_ID head;          // <レコードヘッダー>
      public string HansyokuNum;      // 繁殖登録番号
      public string KeitoId;          // 系統ID
      public string KeitoName;        // 系統名

      public string KeitoEx;          // 系統説明

      public string crlf;             // レコード区切り

      // データセット
      public void SetDataB(ref string strBuff)
      {
        byte[] bBuff = new byte[6887];
        bBuff = Str2Byte(ref strBuff);

        head.SetDataB(MidB2B(ref bBuff, 1, 11));
        HansyokuNum = MidB2S(ref bBuff, 12, 8);
        KeitoId = MidB2S(ref bBuff, 20, 30);
        KeitoName = MidB2S(ref bBuff, 50, 36);
        KeitoEx = MidB2S(ref bBuff, 86, 6800);
        crlf = MidB2S(ref bBuff, 6886, 2);
        bBuff = null;
      }
    }

    #endregion

    #region 30.コース情報

    public struct JV_CS_COURSE
    {
      public RECORD_ID head;      // <レコードヘッダー>
      public string JyoCD;        // 競馬場コード

      public string Kyori;        // 距離
      public string TrackCD;      // トラックコード

      public YMD KaishuDate;      // コース改修年月日
      public string CourseEx;     // コース説明

      public string crlf;         // レコード区切り

      // データセット
      public void SetDataB(ref string strBuff)
      {
        byte[] bBuff = new byte[6829];
        bBuff = Str2Byte(ref strBuff);

        head.SetDataB(MidB2B(ref bBuff, 1, 11));
        JyoCD = MidB2S(ref bBuff, 12, 2);
        Kyori = MidB2S(ref bBuff, 14, 4);
        TrackCD = MidB2S(ref bBuff, 18, 2);
        KaishuDate.SetDataB(MidB2B(ref bBuff, 20, 8));
        CourseEx = MidB2S(ref bBuff, 28, 6800);
        crlf = MidB2S(ref bBuff, 6828, 2);
        bBuff = null;
      }
    }

    #endregion

    #region 31.対戦型データマイニング予想

    /// <summary>
    /// 対戦型データマイニング予想
    /// </summary>
    public struct TM_INFO
    {
      public string Umaban;     // 馬番
      public string TMScore;    // 予測スコア


      // データセット
      public void SetDataB(byte[] bBuff)
      {
        Umaban = MidB2S(ref bBuff, 1, 2);
        TMScore = MidB2S(ref bBuff, 3, 4);
      }
    }

    public struct JV_TM_INFO
    {
      public RECORD_ID head;      // <レコードヘッダー>
      public RACE_ID id;          // <競走識別情報>
      public HM MakeHM;           // データ作成時分
      public TM_INFO[] TMInfo;    // <マイニング予想>
      public string crlf;         // レコード区切り

      // 配列の初期化

      public void Initialize()
      {
        TMInfo = new TM_INFO[18];
      }

      // データセット
      public void SetDataB(ref string strBuff)
      {
        Initialize();
        byte[] bBuff = new byte[141];
        bBuff = Str2Byte(ref strBuff);

        head.SetDataB(MidB2B(ref bBuff, 1, 11));
        id.SetDataB(MidB2B(ref bBuff, 12, 16));
        MakeHM.SetDataB(MidB2B(ref bBuff, 28, 4));

        for (int i = 0; i < 18; i++)
        {
          TMInfo[i].SetDataB(MidB2B(ref bBuff, 32 + (6 * i), 6));
        }

        crlf = MidB2S(ref bBuff, 140, 2);
        bBuff = null;
      }
    }

    #endregion

    #region 32.重勝式(WIN5)

    /// <summary>
    /// 重勝式対象レース情報
    /// </summary>
    public struct WF_RACE_INFO
    {
      public string JyoCD;        // 競馬場コード
      public string Kaiji;        // 開催回[第N回]
      public string Nichiji;      // 開催日目[N日目]
      public string RaceNum;      // レース番号

      // データセット
      public void SetDataB(byte[] bBuff)
      {
        JyoCD = MidB2S(ref bBuff, 1, 2);
        Kaiji = MidB2S(ref bBuff, 3, 2);
        Nichiji = MidB2S(ref bBuff, 5, 2);
        RaceNum = MidB2S(ref bBuff, 7, 2);
      }
    }

    /// <summary>
    /// 有効票数情報
    /// </summary>
    public struct WF_YUKO_HYO_INFO
    {
      public string Yuko_Hyo;     // 有効票数


      // データセット
      public void SetDataB(byte[] bBuff)
      {
        Yuko_Hyo = MidB2S(ref bBuff, 1, 11);
      }
    }

    /// <summary>
    /// 重勝式払戻情報
    /// </summary>
    public struct WF_PAY_INFO
    {
      public string Kumiban;      // 組番
      public string Pay;          // 重勝式払戻金
      public string Tekichu_Hyo;  // 的中票数


      // データセット
      public void SetDataB(byte[] bBuff)
      {
        Kumiban = MidB2S(ref bBuff, 1, 10);
        Pay = MidB2S(ref bBuff, 11, 9);
        Tekichu_Hyo = MidB2S(ref bBuff, 20, 10);
      }
    }

    public struct JV_WF_INFO
    {
      public RECORD_ID head;                         // <レコードヘッダー>
      public YMD KaisaiDate;                         // 開催年月日
      public string reserved1;                       // 予備
      public WF_RACE_INFO[] WFRaceInfo;              // <重勝式対象レース情報>
      public string reserved2;                       // 予備
      public string Hatsubai_Hyo;                    // 重勝式発売票数
      public WF_YUKO_HYO_INFO[] WFYukoHyoInfo;       // <有効票数情報>
      public string HenkanFlag;                             // 返還フラグ
      public string FuseiritsuFlag;                         // 不成立フラグ
      public string TekichunashiFlag;                       // 的中無フラグ
      public string COShoki;                                // キャリーオーバー金額初期
      public string COZanDaka;                              // キャリーオーバー金額残高
      public WF_PAY_INFO[] WFPayInfo;                // <重勝式払戻情報>
      public string crlf;                            // レコード区切り

      // 配列の初期化

      public void Initialize()
      {
        WFRaceInfo = new WF_RACE_INFO[5];
        WFYukoHyoInfo = new WF_YUKO_HYO_INFO[5];
        WFPayInfo = new WF_PAY_INFO[243];
      }

      // データセット
      public void SetDataB(ref string strBuff)
      {
        Initialize();
        byte[] bBuff = new byte[7215];
        bBuff = Str2Byte(ref strBuff);

        head.SetDataB(MidB2B(ref bBuff, 1, 11));
        KaisaiDate.SetDataB(MidB2B(ref bBuff, 12, 8));
        reserved1 = MidB2S(ref bBuff, 20, 2);

        for (int i = 0; i < 5; i++)
        {
          WFRaceInfo[i].SetDataB(MidB2B(ref bBuff, 22 + (8 * i), 8));
        }

        reserved2 = MidB2S(ref bBuff, 62, 6);
        Hatsubai_Hyo = MidB2S(ref bBuff, 68, 11);

        for (int i = 0; i < 5; i++)
        {
          WFYukoHyoInfo[i].SetDataB(MidB2B(ref bBuff, 79 + (11 * i), 11));
        }

        HenkanFlag = MidB2S(ref bBuff, 134, 1);
        FuseiritsuFlag = MidB2S(ref bBuff, 135, 1);
        TekichunashiFlag = MidB2S(ref bBuff, 136, 1);
        COShoki = MidB2S(ref bBuff, 137, 15);
        COZanDaka = MidB2S(ref bBuff, 152, 15);

        for (int i = 0; i < 243; i++)
        {
          WFPayInfo[i].SetDataB(MidB2B(ref bBuff, 167 + (29 * i), 29));
        }

        crlf = MidB2S(ref bBuff, 140, 2);
        bBuff = null;
      }
    }

    #endregion

    #region 33.競走馬除外情報

    public struct JV_JG_JOGAIBA
    {
      public RECORD_ID head;            // <レコードヘッダー>
      public RACE_ID id;                // <競走識別情報>
      public string KettoNum;           // 血統登録番号
      public string Bamei;              // 馬名
      public string ShutsubaTohyoJun;   // 出馬投票受付順番
      public string ShussoKubun;        // 出走区分
      public string JogaiJotaiKubun;    // 除外状態区分
      public string crlf;               // レコード区切り

      // データセット
      public void SetDataB(ref string strBuff)
      {
        byte[] bBuff = new byte[80];
        bBuff = Str2Byte(ref strBuff);

        head.SetDataB(MidB2B(ref bBuff, 1, 11));
        id.SetDataB(MidB2B(ref bBuff, 12, 16));
        KettoNum = MidB2S(ref bBuff, 28, 10);
        Bamei = MidB2S(ref bBuff, 38, 36);
        ShutsubaTohyoJun = MidB2S(ref bBuff, 74, 3);
        ShussoKubun = MidB2S(ref bBuff, 77, 1);
        JogaiJotaiKubun = MidB2S(ref bBuff, 78, 1);
        crlf = MidB2S(ref bBuff, 79, 2);
        bBuff = null;
      }
    }

    #endregion


    #endregion
  }
}