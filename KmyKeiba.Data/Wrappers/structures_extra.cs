using System.Text;

#nullable disable

namespace KmyKeiba.JVLink.Wrappers.JVLib
{
  public static partial class JVData_Struct
  {

    #region 32.ウッドチップ調教

    public struct JV_WC_WOODTIP
    {
      public RECORD_ID head;      // <レコードヘッダー>
      public string TresenKubun;  // トレセン区分

      public YMD ChokyoDate;      // 調教年月日
      public string ChokyoTime;   // 調教時刻
      public string KettoNum;     // 血統登録番号
      public string CourseCD;     // コース
      public string Baba;         // 馬場周り
      public string Reserved;

      public string HaronTime10;  // ハロンタイム合計
      public string LapTime10;    // ラップタイム
      public string HaronTime9;   // ハロンタイム合計
      public string LapTime9;     // ラップタイム
      public string HaronTime8;   // ハロンタイム合計
      public string LapTime8;     // ラップタイム
      public string HaronTime7;   // ハロンタイム合計
      public string LapTime7;     // ラップタイム
      public string HaronTime6;   // ハロンタイム合計
      public string LapTime6;     // ラップタイム
      public string HaronTime5;   // ハロンタイム合計
      public string LapTime5;     // ラップタイム
      public string HaronTime4;   // ハロンタイム合計
      public string LapTime4;     // ラップタイム
      public string HaronTime3;   // ハロンタイム合計
      public string LapTime3;     // ラップタイム
      public string HaronTime2;   // ハロンタイム合計
      public string LapTime2;     // ラップタイム
      public string LapTime1;     // ラップタイム
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
        CourseCD = MidB2S(ref bBuff, 35, 1);
        Baba = MidB2S(ref bBuff, 36, 1);
        Reserved = MidB2S(ref bBuff, 37, 1);

        HaronTime10 = MidB2S(ref bBuff, 38, 4);
        LapTime10 = MidB2S(ref bBuff, 42, 3);
        HaronTime9 = MidB2S(ref bBuff, 45, 4);
        LapTime9 = MidB2S(ref bBuff, 49, 3);
        HaronTime8 = MidB2S(ref bBuff, 52, 4);
        LapTime8 = MidB2S(ref bBuff, 56, 3);
        HaronTime7 = MidB2S(ref bBuff, 59, 4);
        LapTime7 = MidB2S(ref bBuff, 63, 3);
        HaronTime6 = MidB2S(ref bBuff, 66, 4);
        LapTime6 = MidB2S(ref bBuff, 70, 3);
        HaronTime5 = MidB2S(ref bBuff, 73, 4);
        LapTime5 = MidB2S(ref bBuff, 77, 3);
        HaronTime4 = MidB2S(ref bBuff, 80, 4);
        LapTime4 = MidB2S(ref bBuff, 84, 3);
        HaronTime3 = MidB2S(ref bBuff, 87, 4);
        LapTime3 = MidB2S(ref bBuff, 91, 3);
        HaronTime2 = MidB2S(ref bBuff, 94, 4);
        LapTime2 = MidB2S(ref bBuff, 98, 3);
        LapTime1 = MidB2S(ref bBuff, 101, 3);
        crlf = MidB2S(ref bBuff, 104, 2);
        bBuff = null;
      }
    }

    #endregion

    #region N.能力試験レース

    public struct JV_NR_NOSI_RACE
    {
      public RECORD_ID head;              // <レコードヘッダー>
      public RACE_ID3 id;                 // <競走識別情報>
      public string YoubiCD;              // 曜日識別コード

      public string Kyori;                // 距離
      public string KyoriBefore;          // 変更前距離
      public string TrackCD;              // トラックコード

      public string TrackCDBefore;        // 変更前トラックコード

      public string CourseKubunCD;        // コース区分

      public string CourseKubunCDBefore;  // 変更前コース区分

      public string HassoTime;            // 発走時刻
      public string HassoTimeBefore;      // 変更前発走時刻
      public string TorokuTosu;           // 登録頭数
      public string SyussoTosu;           // 出走頭数
      public string NyusenTosu;           // 入線頭数
      public TENKO_BABA_INFO TenkoBaba;   // 天候・馬場状態コード

      public string[] LapTime;            // ラップタイム
      public string HaronTimeS3;          // 前３ハロンタイム
      public string HaronTimeS4;          // 前４ハロンタイム
      public string HaronTimeL3;          // 後３ハロンタイム
      public string HaronTimeL4;          // 後４ハロンタイム
      public CORNER_INFO[] CornerInfo;    // <コーナー通過順位>

      public string crlf;                 // レコード区切り

      // 配列の初期化

      public void Initialize()
      {
        LapTime = new string[25];
        CornerInfo = new CORNER_INFO[4];
      }

      // データセット
      public void SetDataB(ref string strBuff)
      {
        Initialize();
        int i;
        byte[] bBuff = new byte[435];
        bBuff = Str2Byte(ref strBuff);

        head.SetDataB(MidB2B(ref bBuff, 1, 11));
        id.SetDataB(MidB2B(ref bBuff, 12, 12));
        YoubiCD = MidB2S(ref bBuff, 24, 1);
        Kyori = MidB2S(ref bBuff, 25, 4);
        KyoriBefore = MidB2S(ref bBuff, 29, 4);
        TrackCD = MidB2S(ref bBuff, 33, 2);
        TrackCDBefore = MidB2S(ref bBuff, 35, 2);
        CourseKubunCD = MidB2S(ref bBuff, 37, 2);
        CourseKubunCDBefore = MidB2S(ref bBuff, 39, 2);

        HassoTime = MidB2S(ref bBuff, 41, 4);
        HassoTimeBefore = MidB2S(ref bBuff, 45, 4);
        TorokuTosu = MidB2S(ref bBuff, 49, 2);
        SyussoTosu = MidB2S(ref bBuff, 51, 2);
        NyusenTosu = MidB2S(ref bBuff, 53, 2);
        TenkoBaba.SetDataB(MidB2B(ref bBuff, 56, 3));

        for (i = 0; i < 25; i++)
        {
          LapTime[i] = MidB2S(ref bBuff, 59 + (3 * i), 3);
        }

        HaronTimeS3 = MidB2S(ref bBuff, 134, 3);
        HaronTimeS4 = MidB2S(ref bBuff, 137, 3);
        HaronTimeL3 = MidB2S(ref bBuff, 140, 3);
        HaronTimeL4 = MidB2S(ref bBuff, 143, 3);

        for (i = 0; i < 4; i++)
        {
          CornerInfo[i].SetDataB(MidB2B(ref bBuff, 146 + (72 * i), 72));
        }

        crlf = MidB2S(ref bBuff, 434, 2);
        bBuff = null;
      }
    }

    #endregion

    #region N.能力試験馬ごと詳細

    /// <summary>
    /// 競走識別情報3
    /// </summary>
    public struct RACE_ID3
    {
      public string Year;         // 開催年
      public string MonthDay;     // 開催月日
      public string JyoCD;        // 競馬場コード

      public string RaceNum;      // レース番号

      // データセット
      public void SetDataB(byte[] bBuff)
      {
        Year = MidB2S(ref bBuff, 1, 4);
        MonthDay = MidB2S(ref bBuff, 5, 4);
        JyoCD = MidB2S(ref bBuff, 9, 2);
        RaceNum = MidB2S(ref bBuff, 11, 2);
      }
    }

    public struct JV_NS_NOSI_UMA
    {
      public RECORD_ID head;                  // <レコードヘッダー>
      public RACE_ID3 id;                     // <競走識別情報>
      public string Umaban;                   // 馬番
      public string KettoNum;                 // 血統登録番号
      public string Bamei;                    // 馬名

      public string SexCD;                    // 性別コード

      public string HinsyuCD;                 // 品種コード

      public string KeiroCD;                  // 毛色コード

      public string Barei;                    // 馬齢

      public string ChokyosiCode;             // 調教師コード

      public string ChokyosiRyakusyo;         // 調教師名略称

      public string Futan;                    // 負担重量

      public string KisyuCode;                // 騎手コード

      public string KisyuCodeBefore;          // 変更前騎手コード

      public string KisyuRyakusyo;            // 騎手名略称
      public string KisyuRyakusyoBefore;      // 変更前騎手名略称

      public string BaTaijyu;                 // 馬体重
      public string ZogenFugo;                // 増減符号
      public string ZogenSa;                  // 増減差
      public string IJyoCD;                   // 異常区分コード

      public string KakuteiJyuni;             // 確定着順

      public string DochakuKubun;             // 同着区分

      public string DochakuTosu;              // 同着頭数
      public string Time;                     // 走破タイム
      public string ChakusaCD;                // 着差コード

      public string ChakusaCDP;               // +着差コード

      public string ChakusaCDPP;              // ++着差コード

      public string NouryokuSyuruiCD;         // 能力試験種類コード
      public string GohiCD;                   // 合否コード
      public string RiyuCD;                   // 不合格理由コード
      public YMD GohiNengappi;                // 合否年月日
      public string AshiiroCD;                // 脚色コード

      public string Jyuni1c;                  // 1コーナーでの順位

      public string Jyuni2c;                  // 2コーナーでの順位

      public string Jyuni3c;                  // 3コーナーでの順位

      public string Jyuni4c;                  // 4コーナーでの順位

      public string HaronTimeL4;              // 後４ハロンタイム
      public string HaronTimeL3;              // 後３ハロンタイム
      public CHAKUUMA_INFO[] ChakuUmaInfo;    // <1着馬(相手馬)情報>

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
        byte[] bBuff = new byte[311];
        bBuff = Str2Byte(ref strBuff);

        head.SetDataB(MidB2B(ref bBuff, 1, 11));
        id.SetDataB(MidB2B(ref bBuff, 12, 12));
        Umaban = MidB2S(ref bBuff, 24, 2);
        KettoNum = MidB2S(ref bBuff, 26, 10);
        Bamei = MidB2S(ref bBuff, 36, 36);
        SexCD = MidB2S(ref bBuff, 72, 1);
        HinsyuCD = MidB2S(ref bBuff, 73, 1);
        KeiroCD = MidB2S(ref bBuff, 74, 2);
        Barei = MidB2S(ref bBuff, 76, 2);
        ChokyosiCode = MidB2S(ref bBuff, 78, 5);
        ChokyosiRyakusyo = MidB2S(ref bBuff, 83, 8);
        Futan = MidB2S(ref bBuff, 91, 3);
        KisyuCode = MidB2S(ref bBuff, 94, 5);
        KisyuCodeBefore = MidB2S(ref bBuff, 99, 5);
        KisyuRyakusyo = MidB2S(ref bBuff, 104, 8);
        KisyuRyakusyoBefore = MidB2S(ref bBuff, 112, 8);
        BaTaijyu = MidB2S(ref bBuff, 120, 3);
        ZogenFugo = MidB2S(ref bBuff, 123, 1);
        ZogenSa = MidB2S(ref bBuff, 124, 3);
        IJyoCD = MidB2S(ref bBuff, 127, 1);
        KakuteiJyuni = MidB2S(ref bBuff, 128, 2);
        DochakuKubun = MidB2S(ref bBuff, 130, 1);
        DochakuTosu = MidB2S(ref bBuff, 131, 1);
        Time = MidB2S(ref bBuff, 132, 4);
        ChakusaCD = MidB2S(ref bBuff, 136, 3);
        ChakusaCDP = MidB2S(ref bBuff, 139, 3);
        ChakusaCDPP = MidB2S(ref bBuff, 142, 3);
        NouryokuSyuruiCD = MidB2S(ref bBuff, 145, 1);
        GohiCD = MidB2S(ref bBuff, 146, 1);
        RiyuCD = MidB2S(ref bBuff, 147, 1);
        GohiNengappi.SetDataB(MidB2B(ref bBuff, 148, 8));
        AshiiroCD = MidB2S(ref bBuff, 156, 1);
        Jyuni1c = MidB2S(ref bBuff, 157, 2);
        Jyuni2c = MidB2S(ref bBuff, 159, 2);
        Jyuni3c = MidB2S(ref bBuff, 161, 2);
        Jyuni4c = MidB2S(ref bBuff, 163, 2);
        HaronTimeL4 = MidB2S(ref bBuff, 165, 3);
        HaronTimeL3 = MidB2S(ref bBuff, 168, 3);

        for (i = 0; i < 3; i++)
        {
          ChakuUmaInfo[i].SetDataB(MidB2B(ref bBuff, 171 + (46 * i), 46));
        }

        KyakusituKubun = MidB2S(ref bBuff, 309, 1);
        crlf = MidB2S(ref bBuff, 310, 2);
        bBuff = null;
      }
    }

    #endregion
  }
}
