[![en](https://img.shields.io/badge/lang-en-red.svg)](./SourceCodeGuide.md)
[![jp](https://img.shields.io/badge/lang-jp-green.svg)](./SourceCodeGuide.jp.md)

# ソースコードのご案内

## ブランチ

* `master`：主なブランチです。
* `develop`：開発用なメインのブランチです。`feature`ブランチ開発した機能は`develop`へマージされます。
* `feature`：機能ごとにブランチを作成して開発します。
* `release`：リリース用のブランチです。リリースバージョンにタグをつけます。

## フォルダ構成

```
./
├ DesignAssets/				// 自分で作った画像とモデルの編集できるファイル
├ Metroidvania/				// Unityのプロジェクト
├ Tools/				// 開発の便利のために作ったツール
    └ checksum/				// ファイルのmd5チェックサムを計算するツール
└ ...
```

## Unityのシーン

全てのシーンが`Assets/Scenes`フォルダの中に入ります。

* `HihiFramework/GameConfig`：ゲームのエントリーポイントです。ゲーム全体に必要なこと(フレームレートの設定、音声、ローカライズモジュールの初期化など)を用意したら、`Landing`シーンに遷移します。
* `HihiFramework/PrefabsUIEnvironment`：Unity EditorのPrefab Modeに入る時、UI Prefabの環境の設定です。
* `Landing`：本番のゲームの最初のシーンです。画面を押すと、`MainMenu`シーンに遷移します。(新しいゲームをする場合だけ、`Game`シーンに遷移します)
* `MainMenu`：探索したいマップを選択するシーンです。マップを選択したら、`Game`シーンに遷移します。
* `Game`：メインゲームのシーンです。マップのJSONファイルを読み込んでから、動的にマップを生成します。
* `MapEditor`：マップを作成と編集するシーンです。マップを編集したら、GUIボタン`Export MapData`を使ってJSONファイルにエクスポートすることが必要です。
* `Sandbox`：キャラクターの動作の確認ために用意したシーンです。`Assets/Scripts/Life/Char/Params_Char`の`Dev Only`フラグを設定したら、キャラクターの動作がテストできます。

## HihiFrameworkについて

* `Assets/Scripts`フォルダの中のスクリプトは、基本的に種類によってちゃんと分けます。その中に、`HihiFramework`というフォルダがあります。
* `HihiFramework`はゲームの機能をモジュール化(例えば、アセット処理、音声、ログ、ローカライズ)し、汎用的なコードの集まりです。
* `HihiFramework`の中にも`Framework`と`Project`二つのフォルダに分けます。
	* `Framework`：ゲームの仕様にかかわらず、どんなゲームにも使えるコードです。この中のコードを変更すべきではないです。
	* `Project`：`Framework`フォルダのコードをもとに、ゲームの仕様によってカスタマイズするコードです。

## その他

* [コードスタンダード](./Metroidvania/Assets/Documents/HihiFramework/CodeStandard.md)
* [Unityプロジェクトのファイルスタンダード](./Metroidvania/Assets/Documents/HihiFramework/UnityProjectFileStandard.md)