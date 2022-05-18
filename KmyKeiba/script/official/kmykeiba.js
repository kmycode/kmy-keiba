﻿
export function KmyKeiba() {
}

KmyKeiba.getTargetRace = function() {
  const json = __currentRace.getJson();
  const data = JSON.parse(json);
  return new Race(data, __currentRace);
}

KmyKeiba.csDateTimeToDate = function(dateTime) {
  return new Date(dateTime);
}


//============================================================
// レース情報
//============================================================

export function Race(data, csobj) {
  this._obj = csobj;

  // レース名。G1や特別・オープンといった、特別に名前のついているレースに設定される。
  // 地方競馬では上記以外にも、協賛レースの場合はその名前が設定される
  // 条件戦（３歳新馬、未勝利など）では、協賛レースでない限り設定されない
  this.name = data.name;

  // レース名。上記に加えて、条件戦など特別な名前の設定されていないレースでも「３歳新馬」などの情報を返す
  this.displayName = data.displayName;

  // レースの格（参照：JRA-VAN Data Lab. コード表 2003.グレードコード）（取得可能な情報例：G1、G3、地方G1、特別、リステッド）
  // それ以外のレースには原則 10 が設定されている（ただし地方競馬では、 0 が設定される場合もある）
  this.grade = data.grade;

  // レースの条件名（現状は地方競馬でのみ設定されている。競馬場によってフォーマットが異なるので頑張ってください）
  this.subjectName = data.subjectName;

  // レースに出場する2歳馬の条件（参照：JRA-VAN Data Lab. コード表 2007.競走条件コード）（取得可能な情報例：３勝クラス、新馬、未勝利、オープン）
  // 現状は中央競馬にのみ設定されている。地方競馬での年齢条件レースはsubjectNameにおさめられているので自力でパースの必要あり
  this.subjectAge2 = data.subjectAge2;

  // レースに出場する3歳馬の条件（参照：JRA-VAN Data Lab. コード表 2007.競走条件コード）（取得可能な情報例：３勝クラス、新馬、未勝利、オープン）
  // 現状は中央競馬にのみ設定されている。地方競馬での年齢条件レースはsubjectNameにおさめられているので自力でパースの必要あり
  this.subjectAge3 = data.subjectAge3;

  // レースに出場する4歳馬の条件（参照：JRA-VAN Data Lab. コード表 2007.競走条件コード）（取得可能な情報例：３勝クラス、新馬、未勝利、オープン）
  // 現状は中央競馬にのみ設定されている。地方競馬での年齢条件レースはsubjectNameにおさめられているので自力でパースの必要あり
  this.subjectAge4 = data.subjectAge4;

  // レースに出場する5歳馬の条件（参照：JRA-VAN Data Lab. コード表 2007.競走条件コード）（取得可能な情報例：３勝クラス、新馬、未勝利、オープン）
  // 現状は中央競馬にのみ設定されている。地方競馬での年齢条件レースはsubjectNameにおさめられているので自力でパースの必要あり
  this.subjectAge5 = data.subjectAge5;

  // レースに出場する最も若い馬の条件（参照：JRA-VAN Data Lab. コード表 2007.競走条件コード）（取得可能な情報例：３勝クラス、新馬、未勝利、オープン）
  // 現状は中央競馬にのみ設定されている。地方競馬での年齢条件レースはsubjectNameにおさめられているので自力でパースの必要あり
  this.subjectAgeYoungest = data.subjectAgeYoungest;

  // コースの番号（参照：JRA-VAN Data Lab. コード表 2001.競馬場コード）
  this.course = data.course;

  // コースの名前（例：福島、札幌、中山、大井、園田、姫路などが文字列で格納。海外競馬場で設定されていないものあり）
  this.courseName = data.courseName;

  // 中央競馬では、同じコースの内側に柵を置いて、コース幅を狭く／内周を少し長くすることがある。特に古馬レースで設定される
  // A、B、Cなどの文字が入る。中央競馬のうち一部の競馬場や地方競馬では設定されない
  this.courseType = data.courseType;

  // その日・競馬場の何回目のレースであるか。「中京3R」の3の部分
  this.raceNumber = data.raceNumber;

  // 天気（参照：JRA-VAN Data Lab. コード表 2011.天候コード）（取得可能な情報例：晴れ、雨）
  // 競馬場より未発表のレース、また地方競馬の発走前全レースでは 0（不明） が設定されている
  this.weather = data.weather;

  // 馬場（参照：JRA-VAN Data Lab. コード表 2010.馬場状態コード）（取得可能な情報例：良、稍重、重、不良）
  // 競馬場より未発表のレース、また地方競馬の発走前全レースでは 0（不明） が設定されている
  this.condition = data.condition;

  // 地面
  // 0:不明、1:芝、2:ダート、3:芝→ダート、4:サンド
  this.ground = data.ground;

  // 競走の種類
  // 0:不明、1:平地、2:障害
  this.type = data.type;

  // コースの向き
  // 0:不明、1:左、2:右、3:直線
  this.direction = data.direction;

  // 距離（単位：メートル）
  this.distance = data.distance;

  // このレースに登録した馬の数（出走取消などを含む）
  this.horsesCount = data.horsesCount;

  // 発走時刻（Dateオブジェクト）
  this.startTime = KmyKeiba.csDateTimeToDate(data.startTime);

  // 本アプリで独自に解析したレース条件。地方競馬の時のみ設定される。subjectNameをパースすることで求めている。
  // 名古屋競馬場などパースのうまくいかない競馬場もあり、精度は保証できない。
  // 中央競馬の場合はundefinedにはならないが、このデータは使わず、上述の値を組み合わせて判別すること
  //
  // 【データ構造】※地方競馬の場合のみ　構造内にあるクラスについては後述
  // {
  //    allClasses:       このレースに出場する全てのクラスの配列
  //    maxClass:         このレースに出場する最も高いクラス
  //    money:            条件として指定された金額。moneySubjectTypeと組み合わせて判別する
  //    moneySubjectType: 0: 不明、1: 以上、2: 以下、3: 未満（未満は本来は以下で指定された数値を含まないが、競馬場によって定義が異なる可能性あり）
  //    isNewHorses:      新馬戦（true/false）
  //    isNotWon:         未勝利戦（true/false）
  //    isLocal:          地方競馬であるか（true/false）
  //    items:            クラス・年齢ごとの条件の配列。下記のオブジェクトが格納されている
  //    [{
  //        cls:          クラス
  //        classSubject: 0: 不明、1: 以上、2: 以下、3: 未満（「B2」クラスで値が 1 の場合は、「B2以上」となる）
  //        level:        レベル（「B3」「C1」の数字部分）
  //        group:        グループ（「C1五」の「五」の部分を数字に変換したもの）
  //        age:          年齢。「3歳」表記にのみ対応。「3上」や「2-3歳」表記には未対応。それぞれ 0 と 3 が設定される可能性がある
  //    }]
  // }
  //
  // 【クラスのコード】
  // 999: A、998: B、997: C、996: D、10: 賞金、20: 年齢
  // 高知競馬などで G クラスが存在する場合があるが、E 以下のクラスは変換されず 0（不明） となる
  this.subject = data.subject;
}

Race.prototype.getHorses = function() {
  const json = this._obj.horses;
  const data = JSON.parse(json);
  return data.map(d => new RaceHorse(d, this._obj));
}


//============================================================
// レース出場馬情報
//============================================================

export function RaceHorse(data, csraceobj) {
  this._raceObj = csraceobj;
  this._obj = undefined;

  // 予想対象のレースに関する情報であるか（true/false）
  // この結果によって、馬のデータでどこが欠損しているかがわかる。詳細は各項目参照
  this.isTargetRace = data.isTargetRace;

  // 馬の名前
  this.name = data.name;

  // 馬のレース当時の年齢
  this.age = data.age;

  // 馬のレース当時の性別
  // 0:不明、1:牡、2:牝、3:騸馬
  this.sex = data.sex;

  // 馬の持ち込み区分。フラグ（ビット加算）形式
  // 1:（抽）　2:「抽」　4:（父）　8:（市）　16:（地）　32:「地」　64:（外）　128:「外」　256:（招）　512:（持）
  this.type = data.type;

  // 馬の色（参照：JRA-VAN Data Lab. コード表 2203.毛色コード）（取得可能な情報例：栗毛、鹿毛、青毛、芦毛）
  this.color = data.color;

  // レースに出る馬の番号
  this.number = data.number;

  // レースに出る馬の枠番
  this.frameNumber = data.frameNumber;

  // レース結果の順位
  // ※予想対象レースでは、予想時点で過去レースであっても設定されない
  this.place = data.place;

  // 着差を 1馬身=100 の数字であらわしたもの
  // 50: 1/2馬身差　　100: 1馬身差　　225: 2 1/4馬身差　　350: 3 1/2馬身差
  // 例外として　1: あたま、2: 同着、3: はな、4: くび、1500: 大差、0: 不明
  // ※予想対象レースでは、予想時点で過去レースであっても設定されない
  this.margin = data.margin;

  // 異常結果（参照：JRA-VAN Data Lab. コード表 2101.異常区分コード）（取得可能な情報例：発走除外、競走中止、失格、降着）
  // ※予想対象レースでは、予想時点で過去レースであっても設定されない
  this.abnormal = data.abnormal;

  // 人気
  this.popular = data.popular;

  // 単勝オッズ　実際の10倍の数字で表現される
  this.odds = data.odds;

  // 複勝オッズ（最大）　実際の10倍の数字
  this.placeOddsMax = data.placeOddsMax;

  // 複勝オッズ（最小）　実際の10倍の数字
  this.placeOddsMin = data.placeOddsMin;

  // 今回のレースで使用された脚質。地方競馬では機械学習で判断
  // 0:不明　1:逃げ　2:先行　3:差し　4:追込
  // ※予想対象レースでは、予想時点で過去レースであっても設定されない
  this.runningStyle = data.runningStyle;

  // 結果のタイム（秒）　実際の10倍の数字
  // ※予想対象レースでは、予想時点で過去レースであっても設定されない
  this.time = data.time;

  // 結果の後３ハロンタイム（秒）　実際の10倍の数字
  // ※予想対象レースでは、予想時点で過去レースであっても設定されない
  this.a3hTime = data.a3hTime;

  // 過去2年の同じ競馬場でのレースと比較したタイムの偏差値（平均が50、標準偏差が10になるようにした値）
  // ※予想対象レースでは、予想時点で過去レースであっても設定されない
  this.timeDeviationValue = data.timeDeviationValue;

  // 過去2年の同じ競馬場でのレースと比較した後3ハロンタイムの偏差値（平均が50、標準偏差が10になるようにした値）
  // ※予想対象レースでは、予想時点で過去レースであっても設定されない
  this.a3hTimeDeviationValue = data.a3hTimeDeviationValue;

  // この馬のレース情報。Race型のオブジェクトが返される
  // ※予想対象レースの場合は設定されない（データが冗長になるため）
  this.race = data.race;

  // 過去レースの情報の入ったオブジェクト。予想対象レースの場合にのみ設定され、それ以外の場合はnullまたはundefinedになる
  //
  // 【データ構造】
  // {
  //   runningStyle:          過去10レースで最も多く使われた脚質
  //   timeDeviationValue:    過去10レース結果のタイム偏差値の中央値
  //   a3hTimeDeviationValue: 過去10レース結果の後3ハロンタイム偏差値の中央値
  //   beforeRaces:           過去全レースデータのオブジェクトの配列（RaceHorse型）
  // }
  this.history = data.history;
}

RaceHorse.prototype.__getObj = function() {
  if (typeof(this._obj) === 'undefined') {
    this._obj = this._raceObj.getHorse(this.number);
  }
  return this._obj;
}