﻿using GameData;
using Localization;
using Player;
using SNetwork;
using UnityEngine;

namespace Zombified_Initiative
{
    public class ZombieComp : MonoBehaviour
    {
        bool menusetup = false;
        TextDataBlock textmenuroot;
        TextDataBlock textallowedpickups;
        TextDataBlock textallowedshare;
        TextDataBlock textstopcommand;
        TextDataBlock textattack;
        TextDataBlock textpickup;
        TextDataBlock textsupply;
        public CommunicationNode mymenu;

        public bool allowedpickups = true;
        public bool allowedshare = true;
        public bool started = false;
        List<PlayerBotActionBase> actionsToRemove = new();
        PlayerAgent myself = null;
        PlayerAIBot myAI = null;

        public PlayerBotActionBase pickupaction;
        public PlayerBotActionBase shareaction;


        public void Initialize()
        {
            this.myself = this.gameObject.GetComponent<PlayerAgent>();
            if (this.myself == null) return;
            if (!this.myself.Owner.IsBot) { Destroy(this); return; }
            this.myAI = this.gameObject.GetComponent<PlayerAIBot>();
            ZombifiedInitiative.L.LogInfo($"initializing zombified comp on {myself.PlayerName} slot {myself.PlayerSlotIndex}..");
            var localizationService = Text.TextLocalizationService.TryCast<GameDataTextLocalizationService>();
            try
            {
                textmenuroot = TextDataBlock.AddBlock(new() { persistentID = 0, internalEnabled = true, SkipLocalization = true, name = this.myself.PlayerName + "menuroot", English = this.myself.PlayerName });
                localizationService.m_texts.Add(textmenuroot.persistentID, textmenuroot.GetText(localizationService.CurrentLanguage));

                textallowedpickups = TextDataBlock.AddBlock(new() { persistentID = 0, internalEnabled = true, SkipLocalization = true, name = this.myself.PlayerName + "pickupperm", English = this.myself.PlayerName + " toggle pickup permission" });
                localizationService.m_texts.Add(textallowedpickups.persistentID, textallowedpickups.GetText(localizationService.CurrentLanguage));

                textallowedshare = TextDataBlock.AddBlock(new() { persistentID = 0, internalEnabled = true, SkipLocalization = true, name = this.myself.PlayerName + "shareperm", English = this.myself.PlayerName + " toggle share permission" });
                localizationService.m_texts.Add(textallowedshare.persistentID, textallowedshare.GetText(localizationService.CurrentLanguage));

                textstopcommand = TextDataBlock.AddBlock(new() { persistentID = 0, internalEnabled = true, SkipLocalization = true, name = this.myself.PlayerName + "stopcommand", English = this.myself.PlayerName + " stop what you are doing" });
                localizationService.m_texts.Add(textstopcommand.persistentID, textstopcommand.GetText(localizationService.CurrentLanguage));

                textattack = TextDataBlock.AddBlock(new() { persistentID = 0, internalEnabled = true, SkipLocalization = true, name = this.myself.PlayerName + "attack", English = this.myself.PlayerName + " attack my target" });
                localizationService.m_texts.Add(textattack.persistentID, textattack.GetText(localizationService.CurrentLanguage));

                textpickup = TextDataBlock.AddBlock(new() { persistentID = 0, internalEnabled = true, SkipLocalization = true, name = this.myself.PlayerName + "pickup", English = this.myself.PlayerName + " pickup resource under my aim" });
                localizationService.m_texts.Add(textpickup.persistentID, textpickup.GetText(localizationService.CurrentLanguage));

                textsupply = TextDataBlock.AddBlock(new() { persistentID = 0, internalEnabled = true, SkipLocalization = true, name = this.myself.PlayerName + "supply", English = this.myself.PlayerName + " supply resource (aimed or me)" });
                localizationService.m_texts.Add(textsupply.persistentID, textsupply.GetText(localizationService.CurrentLanguage));

                this.mymenu = new(this.textmenuroot.persistentID, CommunicationNode.ScriptType.None);
                mymenu.IsLastNode = false;
                mymenu.m_ChildNodes.Add(new CommunicationNode(textallowedpickups.persistentID, CommunicationNode.ScriptType.None));
                mymenu.m_ChildNodes.Add(new CommunicationNode(textallowedshare.persistentID, CommunicationNode.ScriptType.None));
                mymenu.m_ChildNodes.Add(new CommunicationNode(textstopcommand.persistentID, CommunicationNode.ScriptType.None));
                mymenu.m_ChildNodes.Add(new CommunicationNode(textattack.persistentID, CommunicationNode.ScriptType.None));
                mymenu.m_ChildNodes.Add(new CommunicationNode(textpickup.persistentID, CommunicationNode.ScriptType.None));
                mymenu.m_ChildNodes.Add(new CommunicationNode(textsupply.persistentID, CommunicationNode.ScriptType.None));

                mymenu.m_ChildNodes[0].DialogID = 314;
                mymenu.m_ChildNodes[1].DialogID = 314;
                mymenu.m_ChildNodes[2].DialogID = 314;
                mymenu.m_ChildNodes[3].DialogID = 314;
                mymenu.m_ChildNodes[4].DialogID = 314;
                mymenu.m_ChildNodes[5].DialogID = 314;
            }
            catch { }
            if (ZombifiedInitiative.BotTable.Count == 0) ZombifiedInitiative.BotTable.Add(myself.PlayerName, myAI);
            if (!ZombifiedInitiative.BotTable.ContainsKey(myself.PlayerName)) ZombifiedInitiative.BotTable.Add(myself.PlayerName, myAI);
            this.started = true;
        }

        public void OnDestroy()
        {
            if (ZombifiedInitiative.BotTable.ContainsKey(myself.PlayerName)) ZombifiedInitiative.BotTable.Remove(myself.PlayerName);
            mymenu.IsLastNode = true;
        }

        void Update()
        {
            if (!this.started) return;
            if (!this.menusetup && ZombifiedInitiative.rootmenusetup)
            {
                int menunumber = 0;
                bool flag = false;
                // get index of zombified
                for (int num = 0; num < ZombifiedInitiative._menu.m_menu.CurrentNode.ChildNodes[5].m_ChildNodes.Count; num++)
                    if (TextDataBlock.GetBlock(ZombifiedInitiative._menu.m_menu.CurrentNode.ChildNodes[5].m_ChildNodes[num].TextId).English == "Zombified Initiative")
                        menunumber = num;

                // not readding bot if its already somehow in
                for (int num = 0; num < ZombifiedInitiative._menu.m_menu.CurrentNode.ChildNodes[5].m_ChildNodes[menunumber].m_ChildNodes.Count; num++)
                    if (TextDataBlock.GetBlock(ZombifiedInitiative._menu.m_menu.CurrentNode.ChildNodes[5].m_ChildNodes[menunumber].m_ChildNodes[num].TextId).English == myself.PlayerName)
                    {
                        flag = true;
                        ZombifiedInitiative._menu.m_menu.CurrentNode.ChildNodes[5].m_ChildNodes[menunumber].m_ChildNodes[num].IsLastNode = false;
                    }

                if (!flag)
                {
                    ZombifiedInitiative._menu.m_menu.CurrentNode.ChildNodes[5].m_ChildNodes[menunumber].m_ChildNodes.Add(mymenu);
                    for (int num = 0; num < ZombifiedInitiative._menu.m_menu.CurrentNode.ChildNodes[5].m_ChildNodes[menunumber].m_ChildNodes.Count; num++)
                        if (TextDataBlock.GetBlock(ZombifiedInitiative._menu.m_menu.CurrentNode.ChildNodes[5].m_ChildNodes[menunumber].m_ChildNodes[num].TextId).English == myself.PlayerName)
                            ZombifiedInitiative._menu.m_menu.CurrentNode.ChildNodes[5].m_ChildNodes[menunumber].m_ChildNodes[num].IsLastNode = false;
                }
                this.menusetup = true;
            }

            if (!SNet.IsMaster) return;
            if (this.myAI.Actions.Count == 0) return;
            actionsToRemove.Clear();
            foreach (var action in this.myAI.Actions)
            {
                // pickups?
                if (!allowedpickups && action.GetIl2CppType().Name == "PlayerBotActionCollectItem")
                {
                    var descriptor = action.DescBase.Cast<PlayerBotActionCollectItem.Descriptor>();
                    var itemIsDesinfectionPack = descriptor.TargetItem.PublicName == "Disinfection Pack";
                    var itemIsMediPack = descriptor.TargetItem.PublicName == "MediPack";
                    var itemIsAmmoPack = descriptor.TargetItem.PublicName == "Ammo Pack";
                    var itemIsToolRefillPack = descriptor.TargetItem.PublicName == "Tool Refill Pack";

                    var itemIsPack = itemIsToolRefillPack || itemIsAmmoPack || itemIsMediPack || itemIsDesinfectionPack;
                    if (descriptor.Haste < ZombifiedInitiative._manualActionsHaste && itemIsPack)
                    {
                        pickupaction = action;
                        actionsToRemove.Add(action);
                    }
                } // pickups

                // sharing?
                if (!allowedshare && action.GetIl2CppType().Name == "PlayerBotActionShareResourcePack")
                {
                    var descriptor = action.DescBase.Cast<PlayerBotActionShareResourcePack.Descriptor>();
                    if (descriptor.Haste < ZombifiedInitiative._manualActionsHaste)
                    {
                        shareaction = action;
                        actionsToRemove.Add(action);
                    }
                } // share
            } // foreach action

            if (actionsToRemove.Count == 0) return;
            foreach (var action in actionsToRemove)
            {
                this.myAI.Actions.Remove(action);
                ZombifiedInitiative.L.LogInfo($"{this.myself.PlayerName} action {action.GetIl2CppType().Name} was cancelled");
            }
            actionsToRemove.Clear();
        } // update

        public void PreventManualActions()
        {
            if (!this.started) return;
            if (this.myAI.Actions.Count == 0) return;
            if (this.pickupaction != null) this.pickupaction.DescBase.SetCompletionStatus(PlayerBotActionBase.Descriptor.StatusType.Failed);
            if (this.shareaction != null) this.shareaction.DescBase.SetCompletionStatus(PlayerBotActionBase.Descriptor.StatusType.Failed);
            this.pickupaction = null;
            this.shareaction = null;

            var actionsToRemove = new List<PlayerBotActionBase>();
            var haste = ZombifiedInitiative._manualActionsHaste - 0.01f;

            foreach (var action in this.myAI.Actions)
            {
                if (action.GetIl2CppType().Name == "PlayerBotActionAttack")
                {
                    var descriptor = action.DescBase.Cast<PlayerBotActionAttack.Descriptor>();
                    if (descriptor.Haste > haste)
                    {
                        actionsToRemove.Add(action);
                        continue;
                    }
                }
                if (action.GetIl2CppType().Name == "PlayerBotActionCollectItem")
                {
                    var descriptor = action.DescBase.Cast<PlayerBotActionCollectItem.Descriptor>();
                    if (descriptor.Haste > haste)
                    {
                        actionsToRemove.Add(action);
                        continue;
                    }
                }

                if (action.GetIl2CppType().Name == "PlayerBotActionShareResourcePack")
                {
                    var descriptor = action.DescBase.Cast<PlayerBotActionShareResourcePack.Descriptor>();
                    if (descriptor.Haste > haste)
                    {
                        actionsToRemove.Add(action);
                        continue;
                    }
                }
            }

            foreach (var action in actionsToRemove)
            {
                myAI.Actions.Remove(action); // Queued stop
                // this.myAI.StopAction(action.DescBase); // Instant stop
                ZombifiedInitiative.L.LogInfo($"{this.myself.PlayerName}'s manual actions were cancelled");
            }
            actionsToRemove.Clear();
        } // preventmanual

        public void ExecuteBotAction(PlayerBotActionBase.Descriptor descriptor, string message)
        {
            this.myAI.StartAction(descriptor);
            ZombifiedInitiative.L.LogInfo(message);
        }
    }
}