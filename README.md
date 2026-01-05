# maouchan

unity1week 2025-12（お題：「もうひとつ」）で開発したゲーム「がんばれ！まおうちゃん！」のソースコードです。

## ゲーム

**[がんばれ！まおうちゃん！ - unityroom](https://unityroom.com/games/maouchan)**

まおうちゃんが自分の分身を増やして世界征服を目指すクリッカーゲームです。
![ezgif com-optimize](https://github.com/user-attachments/assets/e2cde8a9-886d-4a08-8848-fc9f06a77578)

アート: うたたね くう໒꒱˖✦ [@kuu_UoxoU](https://x.com/kuu_UoxoU)
サウンド: ぱむるん [@pamupamulun](https://x.com/pamupamulun)

## 技術スタック

- **Unity 6**
- **VContainer** - 依存性注入
- **R3** - Reactive Extensions
- **LitMotion** - トゥイーンアニメーション
- **UniTask** - 非同期処理

## アーキテクチャ

MVPパターン（Model-View-Presenter）を採用しています。

```
Model（データ管理）← Pure C# クラス
  ↑
Presenter（制御ロジック）← Pure C# クラス、ITickable
  ↑
View（描画・UI）← MonoBehaviour
```

- **Model**: ゲーム状態を管理（ReactiveProperty使用）
- **Presenter**: ビジネスロジック、VContainerでDI
- **View**: UI表示のみ、FindFirstObjectByTypeで取得

## ディレクトリ構成

```
Assets/Scripts/
├── Game/                  # ゲームロジック
│   ├── Models/            # GameModel, AreaModel, UpgradeModel
│   ├── Presenters/        # GamePresenter, ShopPresenter, etc.
│   ├── Services/          # SaveService, UnityroomScoreService
│   └── Effects/           # ParticleManager, PostEffectManager
├── UI/                    # UIビュー（21ファイル）
├── ScriptableObject/      # 設定定義
├── Animation/             # アニメーション制御
├── VContainer/            # DI設定
└── Utils -> my-unity-utils  # 汎用ユーティリティ（サブモジュール）

my-unity-utils/            # git submodule
├── UI/                    # ボタンアニメーション等
├── Core/                  # 拡張メソッド
├── System/                # データ永続化、カメラシェイク等
├── Audio/                 # BGM/SE管理
└── ...
```

## ライセンス

MIT License
