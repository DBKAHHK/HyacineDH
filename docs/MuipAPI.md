<div align="center">
<table>
<td valign="center"><img src="https://github.com/twitter/twemoji/blob/master/assets/72x72/1f1fa-1f1f8.png" width="16"/> English</td>
 
<td valign="center"><a href="MuipAPI_zh-CN.md"><img src="https://em-content.zobj.net/thumbs/120/twitter/351/flag-china_1f1e8-1f1f3.png" width="16"/> 简中</td>
 
<td valign="center"><a href="MuipAPI_zh-TW.md"><img src="https://em-content.zobj.net/thumbs/120/twitter/351/flag-china_1f1e8-1f1f3.png" width="16"/> 繁中</td>
 
<td valign="center"><a href="MuipAPI_ja-JP.md"><img src="https://github.com/twitter/twemoji/blob/master/assets/72x72/1f1ef-1f1f5.png" width="16"/> 日本語</td>
</td>
</table>
</div>

## 💡API Help

- External API call interfaces are supported starting from version 2.3.
- The main interface is the Dispatch interface with an entry point. For example, if your Dispatch is http://127.0.0.1:8080, the request parameters and responses are in JSON format.
- (1) Create Session Interface: http://127.0.0.1:8080/muip/create_session (supports POST)
  - -Optional parameter: key_type (type, only supports PEM or default XML)
  - -Response example:
  ```json
  {
    "code": 0,
    "message": "Created!",
    "data": {
        "rsaPublicKey": "***",
        "sessionId": "***",
        "expireTimeStamp": ***
    }
  }
  ```
- (2) Authorization Interface: http://127.0.0.1:8080/muip/auth_admin (supports POST)
  - -Required parameter 1: SessionId (obtained after requesting the Create Session Interface)
  - -Required parameter 2: admin_key (configured in config.json's MuipServer.AdminKey and encrypted under rsaPublicKey [obtained from Create Session Interface] using RSA [pacs#1])
  - -Response example:
  ```json
  {
    "code": 0,
    "message": "Authorized admin key successfully!",
    "data": {
        "sessionId": "***",
        "expireTimeStamp": ***
    }
  }
  ```
- (3) Command Submission Interface: http://127.0.0.1:8080/muip/exec_cmd (supports POST/GET)
  - -Required parameter 1: SessionId (obtained after requesting the Create Session Interface)
  - -Required parameter 2: Command (the command to be executed, encrypted under rsaPublicKey [obtained from Create Session Interface] using RSA [pacs#1])
  - -Required parameter 3: TargetUid (UID of the player executing the command)
  - -Response example:
    ```json
    {
      "code": 0,
      "message": "Success",
      "data": {
          "sessionId": "***",
          "message": "*** //after base64 encoding
      }
    }
    ```
- (4) Get Server Status Interface: http://127.0.0.1:8080/muip/server_information (supports POST/GET)
  - -Required parameter 1: SessionId (obtained after requesting the Create Session Interface)
  - -Response example:
   ```json
    {
      "code": 0,
      "message": "Success",
      "data": {
          "onlinePlayers": [
              {
                  "uid": 10001,
                  "name": "KEVIN",
                  "headIconId": 208001
              },
              ....
          ],
          "serverTime": 1720626191,
          "maxMemory": 16002.227,
          "usedMemory": 7938.5547,
         "programUsedMemory": 323
      }
    }
    ```
- (5) Get Player Information Interface: http://127.0.0.1:8080/muip/player_information (supports POST/GET)
  - -Required parameter 1: SessionId (obtained after requesting the Create Session Interface)
  - -Required parameter 2: Uid (player UID)
  - -Response example:
   ```json
    {
      "code": 0,
      "message": "Success",
      "data": {
          "uid": 10001,
          "name": "KEVIN",
          "signature": "",
          "headIconId": 208001,
          "curPlaneId": 10001,
          "curFloorId": 10001001,
          "playerStatus": "Explore",
          "stamina": 182,
          "recoveryStamina": 4,
          "assistAvatarList": Array[0],
          "displayAvatarList": Array[0],
          "finishedMainMissionIdList": Array[38],
          "finishedSubMissionIdList": Array[273],
          "acceptedMainMissionIdList": Array[67],
          "acceptedSubMissionIdList": Array[169]
      }
  }
  ```