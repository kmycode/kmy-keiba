import { KmyKeiba } from 'official/kmykeiba.js';
import * as fs from 'official/njcompat_fs.js';

// functionの前にasyncをつけること
// 戻り値は文字列にすること。戻り値がそのままbodyタグの中に入れて表示される
(async function() {

  // 予想対象のレースを取得
  const race = KmyKeiba.getTargetRace();

  // 馬の一覧を取得
  const horses = race.getHorses();

  // 類似レース一覧を取得
  const similarRaces = await horses[1].getRiderSimilarRaceHorsesAsync('distance', 300, 0);

  /*
  // ファイルへ書き込み
  const bloodHorses = await horses[1].getBloodNamesAsync();
  const file = await fs.open('test.txt');
  file.writeFileSync(bloodHorses[0]);
  */

  // 馬の名前を返す
  return `${horses[1].number} 番 ${horses[1].name} 号がいいです（）${race.weather}`;
});
