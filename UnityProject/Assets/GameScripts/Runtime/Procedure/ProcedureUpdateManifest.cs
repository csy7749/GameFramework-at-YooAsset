﻿using System;
using Cysharp.Threading.Tasks;
using UnityGameFramework.Runtime;
using YooAsset;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace GameMain
{
    /// <summary>
    /// 流程 => 用户尝试更新清单
    /// </summary>
    public class ProcedureUpdateManifest : ProcedureBase
    {
        public override bool UseNativeDialog { get; }

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            Log.Info("更新资源清单！！！");

            UILoadMgr.Show(UIDefine.UILoadUpdate, $"更新清单文件...");

            UpdateManifest(procedureOwner).Forget();
        }

        private async UniTaskVoid UpdateManifest(ProcedureOwner procedureOwner)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

            var operation = GameModule.Resource.UpdatePackageManifestAsync(GameModule.Resource.PackageVersion);

            await operation.ToUniTask();

            if (operation.Status == EOperationStatus.Succeed)
            {
                if (GameModule.Resource.PlayMode == EPlayMode.HostPlayMode || GameModule.Resource.PlayMode == EPlayMode.WebPlayMode)
                {
                    //线上最新版本operation.PackageVersion
                    Log.Debug($"Updated package Version : from {GameModule.Resource.GetPackageVersion()} to {GameModule.Resource.PackageVersion}");
                    //注意：保存资源版本号作为下次默认启动的版本!
                    // 如果当前是WebGL或者是边玩边下载直接进入预加载阶段。
                    if (GameModule.Resource.PlayMode == EPlayMode.WebPlayMode ||
                        GameModule.Resource.UpdatableWhilePlaying)
                    {
                        // 边玩边下载还可以拓展首包支持。
                        ChangeState<ProcedurePreload>(procedureOwner);
                        return;
                    }

                    ChangeState<ProcedureCreateDownloader>(procedureOwner);
                    return;
                }

                ChangeState<ProcedurePreload>(procedureOwner);
            }
            else
            {
                Log.Error(operation.Error);

                UILoadTip.ShowMessageBox($"用户尝试更新清单失败！点击确认重试 \n \n <color=#FF0000>原因{operation.Error}</color>", MessageShowType.TwoButton,
                    LoadStyle.StyleEnum.Style_Retry
                    , () => { ChangeState<ProcedureUpdateManifest>(procedureOwner); }, UnityEngine.Application.Quit);
            }
        }
    }
}