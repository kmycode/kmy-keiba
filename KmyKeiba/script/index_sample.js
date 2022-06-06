import { KmyKeiba } from 'official/kmykeiba.js';
import * as fs from 'official/njcompat_fs.js';

// functionの前にasyncをつけること
// 戻り値は文字列にすること。戻り値がそのままbodyタグの中に入れて表示される
(async function() {

  let html = '';

  // 画面に進捗を表示するためのオブジェクトを取得
  const progress = KmyKeiba.getProgress();

  // 予想対象のレースを取得
  const race = KmyKeiba.getTargetRace();

  // 地方競馬であれば予想の対象にしない（30～99が地方、30未満が中央）
  if (race.course >= 30) {
    return;
  }

  // レースに出る馬の一覧を取得
  const horses = race.getHorses();

  // スクリプト実行中に画面に表示する進捗の最大値を設定
  progress.setMaxValue(horses.length);

  const horseData = [];
  for (const horse of horses) {
    const data = {};

    data.name = horse.name;
    data.number = horse.number;
    data.timeDeviationValue = horse.history.timeDeviationValue;
    data.a3hTimeDeviationValue = horse.history.a3hTimeDeviationValue;
    data.placeOddsMin = horse.placeOddsMin;

    // 騎手の同コース・同距離のレース一覧を取得する
    let riderRaces = await horse.getRiderSimilarRaceHorsesAsync('course|distance', 200, 0);

    // 中には中止になったレース、未発走のレースが含まれる場合もあるため、条件を指定して項目を絞る
    riderRaces = riderRaces.filter((r) => r.place >= 1);

    // ゼロ除算防止のため、要素が存在するかで処理を分ける
    if (riderRaces.length > 0) {

      // 勝利数
      var winCount = riderRaces.filter((r) => r.place <= 3).length;

      // 勝率を計算して記録
      var rate = winCount / riderRaces.length * 100;
      data.riderWinRate = rate;

    } else {

      // レースがない場合
      data.riderWinRate = 0;
    }

    // この馬のポイントを計算
    // （※説明のために適当に作ってるので、回収率はぼろぼろです）
    data.point = (data.timeDeviationValue + (data.a3hTimeDeviationValue - 50) * horse.runningStyle / 7) *
      ((data.riderWinRate * 0.3 + 0.7) / 100) +
      (560 - horse.riderWeight) * 0.04 - Math.abs(horse.weightDiff) * 0.4;

    // 計算したデータを配列に入れて、HTMLを生成するときに参照する
    horseData.push(data);

    // 進捗を設定（※前回の進捗設定からここまでにawaitを伴う呼び出しが１つもないと画面に反映されない場合があります）
    progress.setValue(horseData.length);
  }

  // HTMLでテーブルを作成
  html += '<table><tr><th>番号</th><th>名前</th><th>T偏</th><th>A3HT</th><th>騎手複勝</th><th>ポイント</th></tr>';

  for (const horse of horseData) {
    html += `<tr><td>${horse.number}</td><th>${horse.name}</th>`;

    // タイム偏差値を表示
    html += `<td>${horse.timeDeviationValue.toFixed(1)}</td>`;

    // 後３ハロンタイム偏差値を表示
    html += `<td>${horse.a3hTimeDeviationValue.toFixed(1)}</td>`;

    // 騎手勝率を表示する。数字が大きければ色を付ける
    // （styleという変数はここでしか使わないので、中かっこで囲んであとあとの処理で変数を混合しないようにする）
    {
      const style = horse.riderWinRate > 30 ? 'color:lime' : '';
      html += '<td><span style="' + style + '">' + horse.riderWinRate.toFixed(1) + '%</span></td>';
    }

    // ポイントを表示する
    {
      const style = horse.point > 10 ? 'color:lime' : '';
      html += '<td><span style="' + style + '">' + horse.point.toFixed(1) + '</span></td>';
    }

    // 行の終わり
    html += '</tr>'
  }

  // テーブルの終わり
  html += '</table>';

  // 馬のリストをポイントの大きい順に並べ替える
  horseData.sort((a, b) => {
    if (a.point < b.point) return 1;
    if (a.point > b.point) return -1;
    return 0;
  });

  // スクリプト利用者に印や買い目を提案するためのオブジェクトを取得
  const suggestion = KmyKeiba.getSuggestion();

  // ポイントの大きい馬から順に印を提案する
  suggestion.mark(horseData[0].number, 1);
  if (horseData.length >= 2) suggestion.mark(horseData[1].number, 2);
  if (horseData.length >= 3) suggestion.mark(horseData[2].number, 3);
  if (horseData.length >= 5) suggestion.mark(horseData[3].number, 4);
  if (horseData.length >= 7) suggestion.mark(horseData[4].number, 5);

  // 買い目を提案する
  const targets_place = [];
  const targets_quinellaBox = [];
  for (let i = 0; i < Math.min(3, horseData.length); i++) {
    const horse = horseData[i];

    // 複勝オッズが1.4より大きければ、複勝を提案する対象として配列に入れる
    if (horse.placeOddsMin > 14) {
      targets_place.push(horse.number);
    }

    // 馬連BOXを提案する対象として配列に入れる
    targets_quinellaBox.push(horse.number);
  }

  // 複勝を提案する
  if (targets_place.length > 0) {
    suggestion.place(1, targets_place);
  }

  // 馬連BOXを提案する
  if (targets_quinellaBox.length >= 2) {
    suggestion.quinellaBox(1, targets_quinellaBox);
  }

  // ファイルへテキストを書き込み
  //const bloodHorses = await horses[1].getBloodNamesAsync();
  //const file = await fs.open('test.txt');
  //file.writeFileSync(bloodHorses[0]);

  // ファイルからテキストを読み込み
  //const text = file.readFileSync();

  // スタイルの設定が必要な場合などに、ここからHTMLのヘッダを設定可能
  KmyKeiba.setHead('<style>th,td { padding: 4px 2px; border: 1px solid gray; } td { text-align: right; }</style>');

  // 生成したHTMLを返す
  return html;
});
