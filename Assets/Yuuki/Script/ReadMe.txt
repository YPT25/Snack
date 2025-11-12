現在Testという名を冠したスクリプトは文字道理テスト中または改良中なため更新時は報告

CharacterBaseが現在仕様上の都合でゆっぴーと自分のとで2つ存在しているため、のちのちどちらかに統合および適用させる


プレハブ設定手順（Unity Editor）

Projectile（弾丸）

3Dオブジェクト（Sphereなど）を作成。

Rigidbody（Use Gravity: OFF）

Collider（IsTrigger: ON）

Projectile.cs をアタッチ。

NetworkIdentity を追加。

Prefab化して「projectilePrefab」に設定。

RangedBox_Player

MPlayerBaseを継承したこのスクリプトをアタッチ。

muzzlePoint に発射位置（子オブジェクト）を指定。

projectilePrefab に上で作った弾丸プレハブを設定。

Rigidbody と Collider は必須。