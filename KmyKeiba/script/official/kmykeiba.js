
const toClrArray = function (arr) {
  if (typeof(arr) === 'number') {
    arr = [arr];
  }

  var clrArray = __hostFuncs.newArr(arr.length);
  for (var i = 0; i < arr.length; ++i) {
    clrArray[i] = arr[i];
  }
  return clrArray;
};



export function KmyKeiba() {
}

// 予想対象レースを取得する
KmyKeiba.getTargetRace = function() {
  const json = __currentRace.item.getJson();
  const data = JSON.parse(json);
  return new Race(data, __currentRace.item);
}

// 予想対象レースを取得する
KmyKeiba.getTargetRaceWithResults = function () {
    const json = __currentRaceWithResults.item.getJson();
    const data = JSON.parse(json);
    return new Race(data, __currentRaceWithResults.item);
}

KmyKeiba.getProgress = function() {
  return Progress;
}

KmyKeiba.getSuggestion = function() {
  return Suggestion;
}

KmyKeiba.setHead = function(text) {
  __html.item.head = text;
}

KmyKeiba.isBulk = function () {
  return __bulk.item.isBulk;
}

// 一括実行で中央競馬を予想するか。falseならスキップ
KmyKeiba.setBulkCentral = function(value) {
  __bulk.item.isCentral = value;
}

// 一括実行で地方競馬（帯広ばんえいを含む）を予想するか。falseならスキップ
KmyKeiba.setBulkLocal = function(value) {
  __bulk.item.isLocal = value;
}

// 一括実行で帯広ばんえい競馬を予想するか。falseならスキップ
// なおこれがtrueでも、setBulkLocalにfalseを設定するとこっちの値は無効になる
KmyKeiba.setBulkBanei = function(value) {
  __bulk.item.isBanei = value;
}

KmyKeiba.mlTraining = function() {
  return new MLTraining();
}

KmyKeiba.mlPrediction = function() {
  return new MLPrediction();
}

KmyKeiba.__csDateTimeToDate = function(dateTime) {
  return new Date(dateTime);
}


function MLTraining(obj) {
  if (!obj) {
    this._obj = __ml.item;
  } else {
    this._obj = obj;
  }
}

MLTraining.prototype.profile = function (name) {
  return new MLTraining(this._obj.profile(name));
}

MLTraining.prototype.addRow = function(data, result) {
  this._obj.addRow(JSON.stringify(data), result);
}

MLTraining.prototype.predictAsync = async function() {
  // dummy
  return [];
}

function MLPrediction(obj) {
  if (!obj) {
    this._obj = __mlp.item;
  } else {
    this._obj = obj;
  }
}

MLPrediction.prototype.profile = function (name) {
  return new MLPrediction(this._obj.profile(name));
}

MLPrediction.prototype.addRow = function(data) {
  this._obj.addRow(JSON.stringify(data));
}

MLPrediction.prototype.predictAsync = async function () {
  return JSON.parse(await this._obj.predictAsync());
}


function Progress() {
}

Progress.setMaxValue = function(value) {
  __html.item.progressMax = value;
}

Progress.setValue = function(value) {
  __html.item.progress = value;
}


function Suggestion() {
}

// 馬にしるしをつける
//   num:  馬の番号
//   mark: 印　0:つけない、1:二重丸、2:丸、3:塗三角、4:三角、5:星、6:消し
Suggestion.mark = function(num, mark) {
  __suggestion.item.mark(num, mark);
}

// 単勝馬券を提案する
//  count: 購入量　1で1枚（100円）
//  nums:  馬の番号の配列
Suggestion.single = function(count, nums) {
  __suggestion.item.single(1, count, toClrArray(nums));
}

// 複勝馬券を提案する
//  count: 購入量　1で1枚（100円）
//  nums:  馬の番号の配列
Suggestion.place = function(count, nums) {
  __suggestion.item.single(2, count, toClrArray(nums));
}

// 枠連馬券を提案する
//  count: 購入量　1で1枚（100円）
//  nums:  枠番号の配列
Suggestion.frame = function(count, nums1, nums2) {
  __suggestion.item.frame(3, count, toClrArray(nums1), toClrArray(nums2));
}

// 枠連BOX馬券を提案する
//  count: 購入量　1で1枚（100円）
//  nums:  馬の番号の配列
Suggestion.frameBox = function(count, nums1) {
  __suggestion.item.frame(2, count, toClrArray(nums1), toClrArray(0));
}

// ワイド馬券を提案する
//  count: 購入量　1で1枚（100円）
//  nums:  馬の番号の配列
Suggestion.quinellaPlace = function(count, nums1, nums2) {
  __suggestion.item.quinellaPlace(3, count, toClrArray(nums1), toClrArray(nums2));
}

// ワイドBOX馬券を提案する
//  count: 購入量　1で1枚（100円）
//  nums:  馬の番号の配列
Suggestion.quinellaPlaceBox = function(count, nums1) {
  __suggestion.item.quinellaPlace(2, count, toClrArray(nums1), toClrArray(0));
}

// 馬連馬券を提案する
//  count: 購入量　1で1枚（100円）
//  nums:  馬の番号の配列
Suggestion.quinella = function(count, nums1, nums2) {
  __suggestion.item.quinella(3, count, toClrArray(nums1), toClrArray(nums2));
}

// 馬連BOX馬券を提案する
//  count: 購入量　1で1枚（100円）
//  nums:  馬の番号の配列
Suggestion.quinellaBox = function(count, nums1) {
  __suggestion.item.quinella(2, count, toClrArray(nums1), toClrArray(0));
}

// 馬単馬券を提案する
//  count: 購入量　1で1枚（100円）
//  nums:  馬の番号の配列
Suggestion.exacta = function(count, nums1, nums2, isMulti) {
  // isMulti ? true : falseは、isMultiがundefinedだった場合を想定
  __suggestion.item.exacta(3, count, isMulti ? true : false, toClrArray(nums1), toClrArray(nums2));
}

// 馬単BOX馬券を提案する
//  count: 購入量　1で1枚（100円）
//  nums:  馬の番号の配列
Suggestion.exactaBox = function(count, nums1, isMulti) {
  __suggestion.item.exacta(2, count, isMulti ? true : false, toClrArray(nums1), toClrArray(0));
}

// 三連複馬券を提案する
//  count: 購入量　1で1枚（100円）
//  nums:  馬の番号の配列
Suggestion.trio = function(count, nums1, nums2, nums3) {
  __suggestion.item.trio(3, count, toClrArray(nums1), toClrArray(nums2), toClrArray(nums3));
}

// 三連複BOX馬券を提案する
//  count: 購入量　1で1枚（100円）
//  nums:  馬の番号の配列
Suggestion.trioBox = function(count, nums1) {
  __suggestion.item.trio(2, count, toClrArray(nums1), toClrArray(0), toClrArray(0));
}

// 三連複流し馬券を提案する
//  count: 購入量　1で1枚（100円）
//  nums:  馬の番号の配列
Suggestion.trioNagashi = function(count, nums1, nums2) {
  __suggestion.item.trio(4, count, toClrArray(nums1), toClrArray(nums2), toClrArray(0));
}

// 三連単馬券を提案する
//  count: 購入量　1で1枚（100円）
//  nums:  馬の番号の配列
Suggestion.trifecta = function(count, nums1, nums2, nums3, isMulti) {
  __suggestion.item.trifecta(3, count, isMulti, toClrArray(nums1), toClrArray(nums2), toClrArray(nums3));
}

// 三連単BOX馬券を提案する
//  count: 購入量　1で1枚（100円）
//  nums:  馬の番号の配列
Suggestion.trifectaBox = function(count, nums1, isMulti) {
  __suggestion.item.trifecta(2, count, isMulti ? true : false, toClrArray(nums1), toClrArray(0), toClrArray(0));
}

// 三連単流し馬券を提案する
//  count: 購入量　1で1枚（100円）
//  nums:  馬の番号の配列
Suggestion.trifectaNagashi = function(count, nums1, nums2, isMulti) {
  __suggestion.item.trifecta(4, count, isMulti ? true : false, toClrArray(nums1), toClrArray(nums2), toClrArray(0));
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

  // レース名。上記に加えて、条件戦など特別な名前の設定されていないレースでも「３歳新馬」などの文字列を返す
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
  this.startTime = KmyKeiba.__csDateTimeToDate(data.startTime);

  // 各コーナーの順位。文字列。例：(*1,6)-(2,7)=(4,8)3,5
  // 変数名に1,2,3,4の数字がついているが、ゴールに近いほど数字が大きい。距離の短いレースでは1～3が設定されていないこともある
  // ※予想対象レースでは常にundefined
  this.cornerRanking1 = data.cornerRanking1;
  this.cornerRanking2 = data.cornerRanking2;
  this.cornerRanking3 = data.cornerRanking3;
  this.cornerRanking4 = data.cornerRanking4;

  // 各コーナーのラップタイム。秒数に10をかけた値が入れられる
  // 変数名に1,2,3,4の数字がついているが、ゴールに近いほど数字が大きい。距離の短いレースでは1～3が設定されていないこともある
  // ※予想対象レースでは常にundefined
  this.cornerLapTime1 = data.cornerLapTime1;
  this.cornerLapTime2 = data.cornerLapTime2;
  this.cornerLapTime3 = data.cornerLapTime3;
  this.cornerLapTime4 = data.cornerLapTime4;

  // このレースの上位5頭の馬データ。RaceHorse型
  // ※以下の場合にのみ設定される。それ以外はnull
  //       getSimilarRacesAsync やその他の getSimilar 系メソッドで取得したレース
  //       予想対象レースの出場馬の過去レース (getHorses()[0].history.beforeRaces[0].race.topHorses)
  this.topHorses = data.topHorses;

  // 本アプリで独自に解析したレース条件。地方競馬の時のみ設定される。subjectNameをパースすることで求めている。
  // 名古屋競馬場などパースのうまくいかない競馬場もあり、精度は保証できない。
  // 中央競馬の場合はnullになるので、このデータは使わず、上述の値を組み合わせて判別すること
  //
  // 【データ構造】※地方競馬の場合のみ　構造内にあるクラスについては後述
  // {
  //    allClasses:       このレースに出場する全てのクラスの配列
  //    maxClass:         このレースに出場する最も高いクラス
  //    money:            条件として指定された金額。円単位で、100万の場合「1000000」が入る（72.5万など小数になる場合の対策）。moneySubjectTypeと組み合わせて判別する
  //    moneySubjectType: 0: 不明、1: 以上、2: 以下、3: 未満（未満は本来は以下と異なり指定された数値を含まないが、競馬場によって定義が異なる可能性あり）
  //    isDebut:          新馬戦（true/false）
  //    isMaiden:         未勝利戦（true/false）
  //    isLocal:          地方競馬であるか（true/false）なおこのオブジェクトは中央競馬の時はnullになるので、スクリプトから見えるisLocalは常にtrueになる
  //    items:            クラス・年齢ごとの条件の配列。下記のオブジェクトが格納されている
  //    [{
  //        cls:          クラス
  //        classSubject: 0: 不明、1: 以上、2: 以下、3: 未満（例：「B2」クラスで値が 1 の場合は「B2以上」となる）
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

// 今回のレースと類似点のある過去レースデータを取得する
//
//   keys:  以下の組み合わせを「|」で区切って指定する
//          例えば「同じ競馬場＆距離」のレースを取得したい場合、「course|distance」を指定する
//             course     同じ競馬場
//             ground     同じ地面（芝、ダート）
//             condition  同じ馬場状態
//             weather    同じ天気
//             name       同じレース名（地方競馬の協賛レースなどでは誤動作の場合あり。条件レースなど名前の設定されないレースでは、同様に名前のない全てのレースを取得）
//             subject    同じ条件（地方競馬では誤動作の場合あり）
//             grade      同じ格（地方競馬では、特に一般レースで誤動作の場合あり。0と10のどちらが設定されるかが競馬場によって違うため）
//             month      同じ月
//             distance   前後100メートルの距離
//             direction  同じ向き（右、左、直線）
//   count:  取得最大数
//   offset: 取得を開始する位置。0を指定すると最新のものから順に取得される
//
// 結果は RaceHorse 型の配列
// ※isTargetRace が false であれば、このメソッドは実行できない
Race.prototype.getSimilarRacesAsync = async function(keys, count, offset) {
  const json = await this._obj.getSimilarRacesAsync(keys, count || 300, offset || 0);
  const data = JSON.parse(json);
  return data.map(d => new Race(d));
}

// 今回のレースと類似点のある過去レースデータに出場した馬一覧を取得する
//
//   keys:  以下の組み合わせを「|」で区切って指定する
//          例えば「同じ競馬場＆距離」のレースを取得したい場合、「course|distance」を指定する
//             course     同じ競馬場
//             ground     同じ地面（芝、ダート）
//             condition  同じ馬場状態
//             weather    同じ天気
//             name       同じレース名（条件レースなど名前の設定されないレースでは、同様に名前のない全てのレースを取得）
//             subject    同じ条件
//             grade      同じ格（地方競馬では、特に一般レースで誤動作の場合あり。0と10のどちらが設定されるかが競馬場によって違うため）
//             month      同じ月
//             distance   前後100メートルの距離
//             direction  同じ向き（右、左、直線）
//             placebits  複勝
//             losed      着外
//             inside     内枠
//             intermediate中枠
//             outside    外枠
//           以下はいずれか１つまたは複数を指定すると絞り込まれる
//             sex_male
//             sex_female
//             sex_castrated
//             interval_1_15
//             interval_16_30
//             interval_31_60
//             interval_61_90
//             interval_91_150
//             interval_151_300
//             interval_301_
//             rs_frontrunner 逃げ
//             rs_stalker     先行
//             rs_sotp        差し
//             rs_saverunner  追込
//   count:  取得最大数
//   offset: 取得を開始する位置。0を指定すると最新のものから順に取得される
//
// 結果は RaceHorse 型の配列
// ※isTargetRace が false であれば、このメソッドは実行できない
Race.prototype.getSimilarRaceHorsesAsync = async function (keys, count, offset) {
    const json = await this._obj.getSimilarRaceHorsesAsync(keys, count || 300, offset || 0);
    const data = JSON.parse(json);
    return data.map(d => new RaceHorse(d));
}

// 以下はオッズ取得関数が並んでいるが、
// 単勝／複勝オッズは各馬（RaceHorse型）のデータの中に入っているのでそれを参照する

// 枠連のオッズ情報を取得
// 結果は以下のオブジェクトの配列として返される
//  {
//    frame1: 枠番1
//    frame2: 枠番2
//    odds:   オッズ（実際の10倍の数値）
//  }
// ※isTargetRace が false であれば、このメソッドは実行できない
Race.prototype.getFrameNumberOdds = function() {
  const json = this._obj.getFrameNumberOdds();
  const data = JSON.parse(json);
  return data;
}

// ワイドのオッズ情報を取得
// 結果は以下のオブジェクトの配列として返される
//  {
//    number1: 馬番1
//    number2: 馬番2
//    oddsMin: オッズ下限（実際の10倍の数値）
//    oddsMax: オッズ上限（実際の10倍の数値）
//  }
// ※isTargetRace が false であれば、このメソッドは実行できない
Race.prototype.getQuinellaPlaceOdds = function() {
  const json = this._obj.getQuinellaPlaceOdds();
  const data = JSON.parse(json);
  return data;
}

// 馬連のオッズ情報を取得
// 結果は以下のオブジェクトの配列として返される
//  {
//    number1: 馬番1
//    number2: 馬番2
//    odds   : オッズ（実際の10倍の数値）
//  }
// ※isTargetRace が false であれば、このメソッドは実行できない
Race.prototype.getQuinellaOdds = function() {
  const json = this._obj.getQuinellaOdds();
  const data = JSON.parse(json);
  return data;
}

// 馬単のオッズ情報を取得
// 結果は以下のオブジェクトの配列として返される
//  {
//    number1: 馬番1
//    number2: 馬番2
//    odds   : オッズ（実際の10倍の数値）
//  }
// ※isTargetRace が false であれば、このメソッドは実行できない
Race.prototype.getExactaOdds = function() {
  const json = this._obj.getExactaOdds();
  const data = JSON.parse(json);
  return data;
}

// 三連複のオッズ情報を取得
// 結果は以下のオブジェクトの配列として返される
//  {
//    number1: 馬番1
//    number2: 馬番2
//    number3: 馬番3
//    odds   : オッズ（実際の10倍の数値）
//  }
// ※isTargetRace が false であれば、このメソッドは実行できない
Race.prototype.getTrioOdds = function() {
  const json = this._obj.getTrioOdds();
  const data = JSON.parse(json);
  return data;
}

// 三連単のオッズ情報を取得
// 結果は以下のオブジェクトの配列として返される
//  {
//    number1: 馬番1
//    number2: 馬番2
//    number3: 馬番3
//    odds   : オッズ（実際の10倍の数値）
//  }
// ※isTargetRace が false であれば、このメソッドは実行できない
Race.prototype.getTrifectaOdds = function() {
  const json = this._obj.getTrifectaOdds();
  const data = JSON.parse(json);
  return data;
}


//============================================================
// レース出場馬情報
//============================================================

export function RaceHorse(data, csraceobj) {
  this._raceObj = csraceobj;
  this._obj = undefined;

  // 予想対象のレースに関する情報であるか（true/false）
  // この結果によって、馬のデータでどこが欠損しているかがわかる。詳細は各項目参照
  // falseであるときに呼び出せないメソッドがある（メソッドそのものが存在しない）ので注意すること
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
  // 例：20 の場合、20を２進数に直すと10100となり、100 (4) + 10000 (16) と等しいので「（父）（地）」になる
  //     ビット演算をすれば簡単に計算できます。ビット演算のやり方は各自調べてください
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

  // コーナー別の順位。位置の近い馬は同じ順位にされるため、複数の馬が同じ数字になっていることも多い。
  // 変数名に1,2,3,4の数字がついているが、ゴールに近いほど数字が大きい。距離の短いレースでは1～3が設定されていないこともある
  this.placeCorner1 = data.placeCorner1;
  this.placeCorner2 = data.placeCorner2;
  this.placeCorner3 = data.placeCorner3;
  this.placeCorner4 = data.placeCorner4;

  // 人気
  this.popular = data.popular;

  // 単勝オッズ　実際の10倍の数字で表現される
  this.odds = data.odds;

  // 複勝オッズ（最大）　実際の10倍の数字
  this.placeOddsMax = data.placeOddsMax;

  // 複勝オッズ（最小）　実際の10倍の数字
  this.placeOddsMin = data.placeOddsMin;

  // メモ
  this.memo = data.memo;

  // 体重。値は実際の10倍
  // ※発表されるまではゼロ
  this.weight = data.weight;

  // 前回のレースからの重量比較。値は実際の10倍ではない
  // ※発表されるまではゼロ
  this.weightDiff = data.weightDiff;

  // 斤量。値は実際の10倍
  this.riderWeight = data.riderWeight;

  // 騎手の同じ地面・近い距離での直近１年間の複勝率
  // ※予想対象レースでのみ取得可能
  this.riderPlaceBitsRate = data.riderPlaceBitsRate;

  // ブリンカーをつけているか
  this.isBlinkers = data.isBlinkers;

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
  // ※過去レースの上位5頭（RaceオブジェクトのtopHorses）からたどってきた場合、この値は設定されない
  this.timeDeviationValue = data.timeDeviationValue;

  // 過去2年の同じ競馬場でのレースと比較した後3ハロンタイムの偏差値（平均が50、標準偏差が10になるようにした値）
  // ※予想対象レースでは、予想時点で過去レースであっても設定されない
  // ※過去レースの上位5頭（RaceオブジェクトのtopHorses）からたどってきた場合、この値は設定されない
  this.a3hTimeDeviationValue = data.a3hTimeDeviationValue;

  // 過去2年の同じ競馬場でのレースと比較した後3ハロンタイムに到達するまでの距離の偏差値（平均が50、標準偏差が10になるようにした値）
  // ※予想対象レースでは、予想時点で過去レースであっても設定されない
  // ※過去レースの上位5頭（RaceオブジェクトのtopHorses）からたどってきた場合、この値は設定されない
  // ※距離が800メートル未満のレースでは設定されない
  this.ua3hTimeDeviationValue = data.ua3hTimeDeviationValue;

  // この馬のレース情報。Race型のオブジェクトが返される
  // ※予想対象レースの場合は設定されない（データが冗長になるため）
  // ※getSimilarRacesAsync で取得したレースでは設定されない（循環参照になるため）
  //   getRiderSimilarRacesAsync / getTrainerSimilarRacesAsync / getBloodHorseRacesAsync では設定される
  this.race = data.race;

  // 過去レースの情報の入ったオブジェクト。予想対象レースの場合にのみ設定され、それ以外の場合はnullまたはundefinedになる
  // 過去10レースよりも前の結果を参照した値を取得したいときは、現状ではhistory.beforeRacesから自分で計算する
  //
  // 【データ構造】
  // {
  //    runningStyle:           過去10レースで最も多く使われた脚質
  //    timeDeviationValue:     過去10レース結果のタイム偏差値の中央値
  //    a3hTimeDeviationValue:  過去10レース結果の後3ハロンタイム偏差値の中央値
  //    ua3hTimeDeviationValue: 過去10レース結果の後3ハロンに到達するまでのタイム偏差値の中央値
  //    disturbanceRate         過去10レースの結果から算出した乱調度
  //    beforeRaces:            過去全レースデータのオブジェクトの配列（RaceHorse型）
  // }
  this.history = data.history;
}

RaceHorse.prototype._getObj = function() {
  if (typeof(this._obj) === 'undefined') {
    this._obj = this._raceObj.getHorse(this.number);
  }
  return this._obj;
}

// 同じ騎手の過去レースデータを取得する
//
//   keys:  以下の組み合わせを「|」で区切って指定する
//          例えば「同じ競馬場＆距離」のレースを取得したい場合、「course|distance」を指定する
//             course         同じ競馬場
//             ground         同じ地面（芝、ダート）
//             condition      同じ馬場状態
//             weather        同じ天気
//             name           同じレース名（地方競馬の協賛レースなどでは誤動作の場合あり。条件レースなど名前の設定されないレースでは、同様に名前のない全てのレースを取得）
//             subject        同じ条件
//             grade          同じ格（地方競馬では、特に一般レースで誤動作の場合あり。0と10のどちらが設定されるかが競馬場によって違うため）
//             month          同じ月
//             distance       前後100メートルの距離
//             direction      同じ向き（右、左、直線）
//             placebits      上位3着以内
//             losed          着外
//             sex            同じ性別
//             frame          同じ枠
//             odds           近いオッズ
//           以下はいずれか１つまたは複数を指定すると絞り込まれる
//             rs_frontrunner 逃げ
//             rs_stalker     先行
//             rs_sotp        差し
//             rs_saverunner  追込
//   count:  取得最大数
//   offset: 取得を開始する位置。0を指定すると最新のものから順に取得される
//
// 結果は RaceHorse 型の配列
// ※isTargetRace が false であれば、このメソッドは実行できない
RaceHorse.prototype.getRiderSimilarRaceHorsesAsync = async function(keys, count, offset) {
  const json = await this._getObj().getRiderSimilarRacesAsync(keys, count || 300, offset || 0);
  const data = JSON.parse(json);
  return data.map(d => new RaceHorse(d));
}

// 同じ調教師の過去レースデータを取得する
//
//   keys:  以下の組み合わせを「|」で区切って指定する
//          例えば「同じ競馬場＆距離」のレースを取得したい場合、「course|distance」を指定する
//             course     同じ競馬場
//             ground     同じ地面（芝、ダート）
//             condition  同じ馬場状態
//             weather    同じ天気
//             name       同じレース名（地方競馬の協賛レースなどでは誤動作の場合あり。条件レースなど名前の設定されないレースでは、同様に名前のない全てのレースを取得）
//             subject    同じ条件
//             grade      同じ格（地方競馬では、特に一般レースで誤動作の場合あり。0と10のどちらが設定されるかが競馬場によって違うため）
//             month      同じ月
//             distance   前後100メートルの距離
//             direction  同じ向き（右、左、直線）
//             placebits  上位3着以内
//             losed      着外
//             sex        同じ性別
//             frame      同じ枠
//   count:  取得最大数
//   offset: 取得を開始する位置。0を指定すると最新のものから順に取得される
//
// 結果は RaceHorse 型の配列
// ※isTargetRace が false であれば、このメソッドは実行できない
RaceHorse.prototype.getTrainerSimilarRaceHorsesAsync = async function(keys, count, offset) {
  const json = await this._getObj().getTrainerSimilarRacesAsync(keys, count || 300, offset || 0);
  const data = JSON.parse(json);
  return data.map(d => new RaceHorse(d));
}

// 指定した血統馬の全レースを取得する
//
//   type: 以下のいずれかから指定する
//          f     父
//          ff    父父
//          fff   父父父
//          ffm   父父母
//          fm    父母
//          fmf   父母父
//          fmm   父母母
//          m     母
//          mf    母父
//          mff   母父父
//          mfm   母父母
//          mm    母母
//          mmf   母母父
//          mmm   母母母
//
// 結果は RaceHorse 型の配列
// ※isTargetRace が false であれば、このメソッドは実行できない
RaceHorse.prototype.getBloodHorseRaceHorsesAsync = async function(type) {
  const json = await this._getObj().getBloodHorseRacesAsync(type);
  const data = JSON.parse(json);
  return data.map(d => new RaceHorse(d));
}

// 指定した血統馬と同じ間柄の馬の過去レースデータを取得する
//
//   type:  getBloodHorseRaceHorsesAsyncのtypeと同一
//   keys:  以下の組み合わせを「|」で区切って指定する
//          例えば「同じ競馬場＆距離」のレースを取得したい場合、「course|distance」を指定する
//          NOTE: 何も指定しないと、読み込み時間が１分を超えることがある。何かを指定することを強く推奨
//             course     同じ競馬場
//             ground     同じ地面（芝、ダート）
//             condition  同じ馬場状態
//             weather    同じ天気
//             name       同じレース名（地方競馬の協賛レースなどでは誤動作の場合あり。条件レースなど名前の設定されないレースでは、同様に名前のない全てのレースを取得）
//             subject    同じ条件
//             grade      同じ格（地方競馬では、特に一般レースで誤動作の場合あり。0と10のどちらが設定されるかが競馬場によって違うため）
//             season     同じ季節
//             distance   前後100メートルの距離
//             direction  同じ向き（右、左、直線）
//             placebits  上位3着以内
//             losed      着外
//             age        同じ年齢
//             grades     重賞
//             frame      同じ枠
//             odds       近いオッズ
//   count:  取得最大数
//   offset: 取得を開始する位置。0を指定すると最新のものから順に取得される
//
// 結果は RaceHorse 型の配列
// ※isTargetRace が false であれば、このメソッドは実行できない
RaceHorse.prototype.getSameBloodHorseRaceHorsesAsync = async function(type, keys, count, offset) {
  const json = await this._getObj().getSameBloodHorseRacesAsync(type, keys, count || 300, offset || 0);
  const data = JSON.parse(json);
  return data.map(d => new RaceHorse(d));
}


// 血統馬の名前を文字列の配列で取得する
// 結果は「getBloodHorseRacesAsync」で示したのと同じ順番で返される
// 例えば結果の[2]は父父父になる
RaceHorse.prototype.getBloodNamesAsync = async function() {
  const json = await this._getObj().getBloodNamesAsync();
  const data = JSON.parse(json);
  return data;
}

// 最近の調教結果を取得する
// 結果は以下のオブジェクトの配列になる
//
// {
//    center:    0:美浦、1:栗東
//    startTime: 開始日時。new Date(startTime)とすることで利用可能
//    course:    ウッドチップ調教の場合に設定
//    direction: ウッドチップ調教の場合に設定。0:右、1:左
//    lapTimes:  ラップタイム（秒）の配列。数字は実際の10倍
// }
RaceHorse.prototype.getTrainings = function() {
  const json = this._getObj().getTrainings();
  const data = JSON.parse(json);
  return data;
}

// 時系列オッズを取得する
// 結果は以下のオブジェクトの配列になる
//
// {
//    time:     時刻
//    odds:     単勝オッズ
// }
RaceHorse.prototype.getOddsTimeline = function () {
    const json = this._getObj().getOddsTimeline();
    const data = JSON.parse(json);
    return data;
}
