猜數字遊戲－windowform

一、透過Socket，實作WebSocket協定。做出猜數字遊戲主機Server與Client
二、隨機數字只有三個，兩名玩家登入且都同意開始遊戲才可以進行遊戲。
三、登入瞬間即代表同意遊戲。
四、當一局遊戲結束了、其中一方登出了，狀態皆為不同意遊戲開始。
	1.雙方都按下了[再玩一次]按鈕。會再次開始遊戲。
	2.一方按下了[再玩一次]按鈕，等待另一方登入。會再次開始遊戲。


--------------------------------------------------

一、Server
1.因為內部的IP PORT是寫死的，所以當第二個Server想要嘗試Initial的時候，第二個Server會跳錯誤訊息視窗並且強制關閉。
2.左邊文字方塊顯示遊戲連線狀態、右邊文字方塊顯示遊戲對玩狀態

二、Client
1.要先按下連線才可以做後續動作。
2.支援多開多連線，但是只能兩個人能登入遊戲房。其他連上線的Client但是沒有登入的Client可以旁觀。
3.必須先以一個遊戲名稱登入，兩個人登入才可以開始遊戲
4.當兩個人登入了Server，遊戲自動開始。先登入的先開始。
5.猜測的過程，所有已連線的Client都會知道。
6.當其中一人猜對了數字，遊戲結束。所有已連線的Client都會知道。
7.遊戲結束之後可以選按[再玩一次]按鈕以等待對方接受再玩一次。所有以連線的Client都會知道。
8.隨時可以登出遊戲，登出遊戲將使該局失效。所有以連線的Client都會知道。
9.要是有人按下了登出，所有已連線的Client都會知道。並且遊戲順序會重製。
	舉例：要是A玩家與B玩家遊戲過程中，B玩家登出。如果A玩家沒有在下個玩家登入前選按[再玩一次]，則下個玩家將取得優先權。
		但要是A玩家先按下了[再玩一次]，則下名玩家登入時，A玩家是優先權。
10.Client斷線會重連(5秒一次)，但並沒有額外做出一個心跳包Thread不斷檢查Server還在不在。所以可能會發生按下了[登入][猜數字]才發現斷線。
11.重連之後必須重新登入，Server並沒有紀錄原本的遊戲紀錄。
