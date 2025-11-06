現在Testという名を冠したスクリプトは文字道理テスト中または改良中なため更新時は報告

CharacterBaseが現在仕様上の都合でゆっぴーと自分のとで2つ存在しているため、のちのちどちらかに統合および適用させる

<使い方＞
＜前準備＞
空のオブジェクトを作り、そこにRespawnManagerをアタッチ
Canvasをつくり、そこにRespawnUIという名前でパネルを作る。（このパネルは非アクティブ化しておく）
RespawnUIにHorizontal Layoutgroupというコンポーネントをつける。
Bottonを作り、ボタンの親子関係になっているテキストを消してプレハブ化しておく。

RespawnManagerコンポーネントのインスペクター内で、リスポーン対象にしたいプレイヤーのプレハブを入れる
プレイヤーにはインスペクター内でリスポーンのボタンに設定す	る画像を入れておく
RespawnPointにはリスポーン地点の座標を設定。
RespawnUIには先ほど作ったRespawnUIを、Button Prefabにはボタンのプレハブを入れる。


ミラー対応

・RespawnManager の配置

シーン上に空のオブジェクトを作成 → 名前を RespawnManager

コンポーネントに RespawnManager.cs を追加

Inspectorを以下のように設定：

フィールド名	設定内容
Player Prefabs	NormalBox_Player（ENEMYプレイヤーPrefab）を登録
Respawn Points	空のオブジェクト（例：RespawnPoint）を作って登録
Respawn UI	リスポーンUI Canvasを登録（非アクティブ状態でOK）
Button Prefab	リスポーンボタンのPrefabを登録

・ NPC の配置（任意）

AI挙動を確認したい場合のみ、以下のPrefabをシーンに配置してOKです：

NormalBox_NPC

SphereBox_NPC