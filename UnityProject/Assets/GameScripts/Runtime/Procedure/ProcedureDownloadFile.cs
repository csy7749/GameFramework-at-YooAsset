﻿using System;
using Cysharp.Threading.Tasks;
using GameFramework;
using UnityEngine;
using UnityGameFramework.Runtime;
using YooAsset;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace GameMain
{
    public class ProcedureDownloadFile:ProcedureBase
    {
        public override bool UseNativeDialog { get; }
        
        private ProcedureOwner _procedureOwner;

        private float _lastUpdateDownloadedSize;
        private float CurrentSpeed
        {
            get
            {
                float interval = Time.deltaTime;
                var sizeDiff = GameModule.Resource.Downloader.CurrentDownloadBytes - _lastUpdateDownloadedSize;
                _lastUpdateDownloadedSize = GameModule.Resource.Downloader.CurrentDownloadBytes;
                var speed = (float)Math.Floor(sizeDiff / interval);
                return speed;
            }
        }
        
        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            _procedureOwner = procedureOwner;
            
            Log.Info("开始下载更新文件！");
            
            UILoadMgr.Show(UIDefine.UILoadUpdate,$"开始下载更新文件...");
            
            BeginDownload().Forget();
        }
        
        private async UniTaskVoid BeginDownload()
        {
            var downloader = GameModule.Resource.Downloader;

            // 注册下载回调
            downloader.DownloadErrorCallback = OnDownloadErrorCallback;
            downloader.DownloadUpdateCallback = OnDownloadProgressCallback;
            downloader.BeginDownload();
            await downloader;

            // 检测下载结果
            if (downloader.Status != EOperationStatus.Succeed)
                return;

            ChangeState<ProcedureDownloadOver>(_procedureOwner);
        }

        private void OnDownloadErrorCallback(DownloadErrorData downloadErrorData)
        {
            UILoadTip.ShowMessageBox($"Failed to download file : {downloadErrorData.FileName}", MessageShowType.TwoButton,
                LoadStyle.StyleEnum.Style_Default
                , () => { ChangeState<ProcedureCreateDownloader>(_procedureOwner); }, UnityEngine.Application.Quit);
        }

        private void OnDownloadProgressCallback(DownloadUpdateData downloadUpdateData)
        {
            string currentSizeMb = (downloadUpdateData.CurrentDownloadBytes / 1048576f).ToString("f1");
            string totalSizeMb = (downloadUpdateData.TotalDownloadBytes / 1048576f).ToString("f1");
            // UILoadMgr.Show(UIDefine.UILoadUpdate,$"{currentDownloadCount}/{totalDownloadCount} {currentSizeMb}MB/{totalSizeMb}MB");
            string descriptionText = Utility.Text.Format("正在更新，已更新{0}，总更新{1}，已更新大小{2}，总更新大小{3}，更新进度{4}，当前网速{5}/s",
                downloadUpdateData.CurrentDownloadCount.ToString(),
                downloadUpdateData.TotalDownloadCount.ToString(),
                Utility.File.GetByteLengthString(downloadUpdateData.CurrentDownloadBytes),
                Utility.File.GetByteLengthString(downloadUpdateData.TotalDownloadBytes),
                GameModule.Resource.Downloader.Progress,
                Utility.File.GetLengthString((int)CurrentSpeed));
            GameEvent.Send(StringId.StringToHash("DownProgress"), GameModule.Resource.Downloader.Progress);
            UILoadMgr.Show(UIDefine.UILoadUpdate, descriptionText);

            int needTime = 0;
            if (CurrentSpeed > 0)
            {
                needTime = (int)((downloadUpdateData.TotalDownloadBytes - downloadUpdateData.CurrentDownloadBytes) / CurrentSpeed);
            }

            TimeSpan ts = new TimeSpan(0, 0, needTime);
            string timeStr = ts.ToString(@"mm\:ss");
            string updateProgress = Utility.Text.Format("剩余时间 {0}({1}/s)", timeStr, Utility.File.GetLengthString((int)CurrentSpeed));
            Log.Info(updateProgress);
        }
    }
}