import { KmyKeiba } from 'official/kmykeiba.js';

// functionの前にasyncをつけること
// 戻り値は文字列にすること。戻り値がそのままbodyタグの中に入れて表示される
(async function() {

  // 予想対象のレースを取得
  const race = KmyKeiba.getTargetRace();

  // 馬の一覧を取得
  const horses = race.getHorses();

  // レースの名前を返す
  return `${horses[1].number} 番 ${horses[1].name} 号がいいです（）`;
});
