2023/01/05
・簡単に作ってみる
	・レイアウト
		Gridを忘れている
			・RowDefinition
			  ColumnDefinition
	・ラベル、テキストボックス、テキストブロック、ボタン
		・テキストボックス変更検知
			TextChanged
				→OK
		・テキストボックスと値を連動させたい
			変更通知
				・Binding.UpdateSourceTriggerはいつ？を表す
					PropertyChanged or LostFocus or Explicit

					・ソースサンプル
					https://github.com/microsoft/WPF-Samples/tree/master/Sample%20Applications/DataBindingDemo
					・Window.Resourcesとかを作る

					・バインディング宣言
					https://learn.microsoft.com/ja-jp/dotnet/desktop/wpf/data/binding-declarations-overview?view=netdesktop-6.0
						PathがPropertyName
				
		・Validationのやり方
			Binding.ValidationRules

		・結局うまくいかず
			・https://zenn.dev/takuty/articles/b12f4011871058
				ここを実装してみる
				・DataContextにViewModel
				・MainWindowViewModel 作ってみる
				・MainWindowModel 作ってみる
				・ConcatCommand 作ってみる
					RequerySuggestedを使ってる
				・ViewModelの中にModelがある
	・データグリッド
		MainWindow2.xaml
			ItemSourceを指定するだけで表示される。
			→OK
	・ListView
		→ほぼ類似
	・BMP
	・SVG
		SharpVectorsを使ってみる			
			パスが悪くて表示されない?
				SVGViewBoxがうまくいかない?
				↓
				実行時に表示されるのを確認した。何度かやってると表示された
					出力時コピー

			MainWindow4
				TutorialSample(FileSvgReaderSample)から取得して作ってみる
					DockPanelとTabItemで
					ソース上から読み込む
					→OK

一旦Upする。
		・もう少しサンプル見る
			MainWindow5に持ってくる
				DockPanel tabControlについか
				・いろんなクラスを移植
				・DrawingPage.xaml作成(Pageであること)
					DockPanel
					SelectFileを用意
				↓
				なんとか表示
				↓
				スタートのZoomは動いた
				↓
				ズームが効いてこない
					OnZoomPanMouseWheel
						→OK
				↓
				MouseUp Downで、ひっぱれない
					→なんとなくOK
				↓
				


		・svgを簡単に作ってみる
			svgsample.svgを用意する

	・Prism+Livlet