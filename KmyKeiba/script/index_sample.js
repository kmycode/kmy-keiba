import { KmyKeiba } from 'official/kmykeiba.js';
import * as fs from 'official/njcompat_fs.js';

// functionの前にasyncをつけること
// 戻り値は文字列にすること。戻り値がそのままbodyタグの中に入れて表示される
(async function() {

  let html = '';
  const progress = KmyKeiba.getProgress();

  // 予想対象のレースを取得
  const race = KmyKeiba.getTargetRace();

  if (race.course >= 30) {
    return;
  }

  // 馬の一覧を取得
  const horses = race.getHorses();

  // 進捗を設定
  progress.setMaxValue(horses.length);

  const horseData = [];
  for (const horse of horses) {
    const data = {};

    data.name = horse.name;
    data.number = horse.number;
    data.timeDeviationValue = horse.history.timeDeviationValue;
    data.a3hTimeDeviationValue = horse.history.a3hTimeDeviationValue;
    data.placeOddsMin = horse.placeOddsMin;

    // 騎手勝率
    const riderRaces = await horse.getRiderSimilarRaceHorsesAsync('course|distance', 200, 0);
    if (riderRaces.length > 0) {
      var winCount = riderRaces.filter((r) => r.place <= 3).length;
      var rate = winCount / riderRaces.length * 100;
      data.riderWinRate = rate;
    } else {
      data.riderWinRate = 0;
    }

    // ポイント
    data.point = (data.timeDeviationValue + (data.a3hTimeDeviationValue - 50) * horse.runningStyle / 7) *
      ((data.riderWinRate * 0.3 + 0.7) / 100) +
      (560 - horse.riderWeight) * 0.04 - Math.abs(horse.weightDiff) * 0.4;

    horseData.push(data);
    progress.setValue(horseData.length);
  }

  html += '<table><tr><th>番号</th><th>名前</th><th>T指数</th><th>A3HT</th><th>騎手複勝</th><th>ポイント</th></tr>';

  for (const horse of horseData) {
    html += `<tr><td>${horse.number}</td><th>${horse.name}</th>`;

    html += `<td>${horse.timeDeviationValue.toFixed(1)}</td>`;
    html += `<td>${horse.a3hTimeDeviationValue.toFixed(1)}</td>`;

    // 騎手勝率
    html += '<td>';
    if (horse.riderWinRate > 30) {
      html += '<span style="color:lime">' + horse.riderWinRate.toFixed(1) + '%</span>';
    } else {
      html += horse.riderWinRate.toFixed(1) + '%';
    }
    html += '</td>';

    // ポイント
    html += '<td>';
    if (horse.point > 10) {
      html += '<span style="color:lime">' + horse.point.toFixed(1) + '</span>';
    } else {
      html += horse.point.toFixed(1);
    }
    html += '</td>';

    html += '</tr>'
  }

  html += '</table>';

  // 印つけ
  horseData.sort((a, b) => {
    if (a.point < b.point) return 1;
    if (a.point > b.point) return -1;
    return 0;
  });

  const suggestion = KmyKeiba.getSuggestion();
  suggestion.mark(horseData[0].number, 1);
  if (horseData.length >= 2) suggestion.mark(horseData[1].number, 2);
  if (horseData.length >= 3) suggestion.mark(horseData[2].number, 3);
  if (horseData.length >= 5) suggestion.mark(horseData[3].number, 4);
  if (horseData.length >= 7) suggestion.mark(horseData[4].number, 5);

  /*
  if (horseData.length >= 7) {
    suggestion.quinella(1,
      [horseData[0].number, horseData[1].number, horseData[2].number],
      [horseData[1].number, horseData[2].number, horseData[3].number, horseData[4].number]);
  } else if (horseData.length >= 5) {
    suggestion.quinella(1,
      [horseData[0].number, horseData[1].number, horseData[2].number],
      [horseData[1].number, horseData[2].number, horseData[3].number]);
  }
  */
  const targets = [];
  const targets2 = [];
  for (let i = 0; i < Math.min(3, horseData.length); i++) {
    const horse = horseData[i];
    if (horse.placeOddsMin > 14) {
      targets.push(horse.number);
    }
    targets2.push(horse.number);
  }
  if (targets.length > 0) {
   suggestion.place(1, targets);
   suggestion.quinellaBox(1, targets2);
  }

  // ファイルへ書き込み
  //const bloodHorses = await horses[1].getBloodNamesAsync();
  //const file = await fs.open('test.txt');
  //file.writeFileSync(bloodHorses[0]);

  KmyKeiba.setHead('<style>th,td { padding: 4px 2px; border: 1px solid gray; } td { text-align: right; }</style>');

  // 馬の名前を返す
  return html;
});
