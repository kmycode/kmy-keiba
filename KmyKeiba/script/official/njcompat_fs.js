//
// 申し訳程度のNode.JSのごく一部関数対応のみです。
// 対応しているものは以下（ちなみにすべて未テスト）
//
// fs             open(path), mkdir(path)
// FileHandle     readFile(), readFileSync(), writeFile(string), writeFileSync(string)
//
// ※以下は未テストです。おそらくバイナリ配列の受け渡し処理がうまくいかず動作しないはずです
// FileHandle     createReadStream(), createWriteStream()
// Stream         on(eventName, action)    eventNameはdata、endのみ対応
//                once(eventName, action), pipe(stream), write(byte[]), end()
//
// 例：
//     const file = await fs.open('test.txt');
//     const text = file.readFile();
//
// Node.JS互換ライブラリは、まじめに作ろうとすると一大プロジェクトになるため優先順位低めです。
// 追加してほしいものがあったら自分でGitHubにプルリクするか、
// KMY競馬の一部ソースをMITライセンスのもとコピペして新しいプロジェクト作ってください
//

export function open(path) {
  return __fs.open(path);
}

export function mkdir(path) {
  return __fs.mkdir(path);
}
