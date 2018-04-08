using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Oxide.Core;
using Oxide.Core.Configuration;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("VKBot", "SkiTles", "1.6.6")]
    class VKBot : RustPlugin
    {
        //Данный плагин принадлежит группе vk.com/vkbotrust
        //Данный плагин предоставляется в существующей форме,
        //"как есть", без каких бы то ни было явных или
        //подразумеваемых гарантий, разработчик не несет
        //ответственность в случае его неправильного использования.

        [PluginReference]
        Plugin Duel;

        #region Variables
        private System.Random random = new System.Random();
        private string msg;
        private string mapfile;
        private bool NewWipe = false;
        JsonSerializerSettings jsonsettings;
        private Dictionary<ulong, ulong> PlayersCheckList = new Dictionary<ulong, ulong>();
        private List<string> allowedentity = new List<string>()
        {
            "door",
            "wall.window.bars.metal",
            "wall.window.bars.toptier",
            "wall.external",
            "gates.external.high",
            "floor.ladder",
            "embrasure",
            "floor.grill",
            "wall.frame.fence",
            "wall.frame.cell"
        };
        private List<ulong> BDayPlayers = new List<ulong>();
        private List<BasePlayer> ConfCods = new List<BasePlayer>();
        #endregion

        #region Config
        private ConfigData config;
        private class ConfigData
        {
            [JsonProperty(PropertyName = "Ключи VK API, ID группы")]
            public VKAPITokens VKAPIT { get; set; }

            [JsonProperty(PropertyName = "Настройки оповещений администраторов")]
            public AdminNotify AdmNotify { get; set; }

            [JsonProperty(PropertyName = "Настройки оповещений в беседу")]
            public ChatNotify ChNotify { get; set; }

            [JsonProperty(PropertyName = "Настройки статуса")]
            public StatusSettings StatusStg { get; set; }

            [JsonProperty(PropertyName = "Оповещения при вайпе")]
            public WipeSettings WipeStg { get; set; }

            [JsonProperty(PropertyName = "Награда за вступление в группу")]
            public GroupGifts GrGifts { get; set; }

            [JsonProperty(PropertyName = "Награда для именинников")]
            public BDayGiftSet BDayGift { get; set; }

            [JsonProperty(PropertyName = "Поддержка нескольких серверов")]
            public MultipleServersSettings MltServSet { get; set; }

            [JsonProperty(PropertyName = "Проверка игроков на читы")]
            public PlayersCheckingSettings PlChkSet { get; set; }

            [JsonProperty(PropertyName = "Топ игроки вайпа и промо")]
            public TopWPlPromoSet TopWPlayersPromo { get; set; }

            [JsonProperty(PropertyName = "Настройки чат команд")]
            public CommandSettings CMDSet { get; set; }

            [JsonProperty(PropertyName = "Динамическая обложка группы")]
            public DynamicGroupLabelSettings DGLSet { get; set; }

            public class VKAPITokens
            {
                [JsonProperty(PropertyName = "VK Token группы (для сообщений)")]
                [DefaultValue("Заполните эти поля, и выполните команду o.reload VKBot")]
                public string VKToken { get; set; } = "Заполните эти поля, и выполните команду o.reload VKBot";

                [JsonProperty(PropertyName = "VK Token приложения (для записей на стене и статуса)")]
                [DefaultValue("Заполните эти поля, и выполните команду o.reload VKBot")]
                public string VKTokenApp { get; set; } = "Заполните эти поля, и выполните команду o.reload VKBot";

                [JsonProperty(PropertyName = "VKID группы")]
                [DefaultValue("Заполните эти поля, и выполните команду o.reload VKBot")]
                public string GroupID { get; set; } = "Заполните эти поля, и выполните команду o.reload VKBot";
            }

            public class AdminNotify
            {
                [JsonProperty(PropertyName = "VkID администраторов (пример /11111, 22222/)")]
                [DefaultValue("Заполните эти поля, и выполните команду o.reload VKBot")]
                public string VkID { get; set; } = "Заполните эти поля, и выполните команду o.reload VKBot";

                [JsonProperty(PropertyName = "Включить отправку сообщений администратору командой /report ?")]
                [DefaultValue(true)]
                public bool SendReports { get; set; } = true;

                [JsonProperty(PropertyName = "Очистка базы репортов при вайпе?")]
                [DefaultValue(true)]
                public bool ReportsWipe { get; set; } = true;

                [JsonProperty(PropertyName = "Предупреждение о злоупотреблении функцией репортов")]
                [DefaultValue("Наличие в тексте нецензурных выражений, оскорблений администрации или игроков сервера, а так же большое количество безсмысленных сообщений приведет к бану!")]
                public string ReportsNotify { get; set; } = "Наличие в тексте нецензурных выражений, оскорблений администрации или игроков сервера, а так же большое количество безсмысленных сообщений приведет к бану!";

                [JsonProperty(PropertyName = "Отправлять сообщение администратору о бане игрока?")]
                [DefaultValue(true)]
                public bool UserBannedMsg { get; set; } = true;

                [JsonProperty(PropertyName = "Отправлять сообщение администратору о нерабочих плагинах?")]
                [DefaultValue(true)]
                public bool PluginsCheckMsg { get; set; } = true;
            }

            public class ChatNotify
            {
                [JsonProperty(PropertyName = "VK Token приложения (лучше использовать отдельную страницу для получения токена)")]
                [DefaultValue("Заполните эти поля, и выполните команду o.reload VKBot")]
                public string ChNotfToken { get; set; } = "Заполните эти поля, и выполните команду o.reload VKBot";

                [JsonProperty(PropertyName = "ID беседы")]
                [DefaultValue("Заполните эти поля, и выполните команду o.reload VKBot")]
                public string ChatID { get; set; } = "Заполните эти поля, и выполните команду o.reload VKBot";

                [JsonProperty(PropertyName = "Включить отправку оповещений в беседу?")]
                [DefaultValue(false)]
                public bool ChNotfEnabled { get; set; } = false;

                [JsonProperty(PropertyName = "Дополнительная отправка оповещений в личку администраторам?")]
                [DefaultValue(false)]
                public bool AdmMsg { get; set; } = false;

                [JsonProperty(PropertyName = "Список оповещений отправляемых в беседу (доступно: reports, wipe, bans, plugins)")]
                [DefaultValue("reports, wipe, oxideupdate, bans")]
                public string ChNotfSet { get; set; } = "reports, wipe, bans, plugins";
            }

            public class StatusSettings
            {
                [JsonProperty(PropertyName = "Обновлять статус в группе? Если стоит /false/ статистика собираться не будет")]
                [DefaultValue(true)]
                public bool UpdateStatus { get; set; } = true;

                [JsonProperty(PropertyName = "Вид статуса (1 - текущий сервер, 2 - список серверов, необходим Rust:IO на каждом сервере)")]
                [DefaultValue(1)]
                public int StatusSet { get; set; } = 1;

                [JsonProperty(PropertyName = "Онлайн в статусе вида '125/200'")]
                [DefaultValue(false)]
                public bool OnlWmaxslots { get; set; } = false;

                [JsonProperty(PropertyName = "Таймер обновления статуса (минуты)")]
                [DefaultValue(30)]
                public int UpdateTimer { get; set; } = 30;

                [JsonProperty(PropertyName = "Формат статуса")]
                [DefaultValue("{usertext}. Сервер вайпнут: {wipedate}. Онлайн игроков: {onlinecounter}. Спящих: {sleepers}. Добыто дерева: {woodcounter}. Добыто серы: {sulfurecounter}. Выпущено ракет: {rocketscounter}. Время обновления: {updatetime}. Использовано взрывчатки: {explosivecounter}. Создано чертежей: {blueprintsconter}. {connect}")]
                public string StatusText { get; set; } = "{usertext}. Сервер вайпнут: {wipedate}. Онлайн игроков: {onlinecounter}. Спящих: {sleepers}. Добыто дерева: {woodcounter}. Добыто серы: {sulfurecounter}. Выпущено ракет: {rocketscounter}. Время обновления: {updatetime}. Использовано взрывчатки: {explosivecounter}. Создано чертежей: {blueprintsconter}. {connect}";

                [JsonProperty(PropertyName = "Список счетчиков, которые будут отображаться в виде emoji")]
                [DefaultValue("onlinecounter, rocketscounter, blueprintsconter, explosivecounter, wipedate")]
                public string EmojiCounterList { get; set; } = "onlinecounter, rocketscounter, blueprintsconter, explosivecounter, wipedate";

                [JsonProperty(PropertyName = "Ссылка на коннект сервера вида /connect 111.111.111.11:11111/")]
                [DefaultValue("connect 111.111.111.11:11111")]
                public string connecturl { get; set; } = "connect 111.111.111.11:11111";

                [JsonProperty(PropertyName = "Текст для статуса")]
                [DefaultValue("Сервер 1")]
                public string StatusUT { get; set; } = "Сервер 1";
            }

            public class WipeSettings
            {
                [JsonProperty(PropertyName = "Отправлять пост в группу после вайпа?")]
                [DefaultValue(false)]
                public bool WPostB { get; set; } = false;

                [JsonProperty(PropertyName = "Текст поста о вайпе")]
                [DefaultValue("Заполните эти поля, и выполните команду o.reload VKBot")]
                public string WPostMsg { get; set; } = "Заполните эти поля, и выполните команду o.reload VKBot";

                [JsonProperty(PropertyName = "Добавить изображение к посту о вайпе?")]
                [DefaultValue(false)]
                public bool WPostAttB { get; set; } = false;

                [JsonProperty(PropertyName = "Ссылка на изображение к посту о вайпе вида 'photo-1_265827614' (изображение должно быть в альбоме группы)")]
                [DefaultValue("photo-1_265827614")]
                public string WPostAtt { get; set; } = "photo-1_265827614";

                [JsonProperty(PropertyName = "Отправлять сообщение администратору о вайпе?")]
                [DefaultValue(true)]
                public bool WPostMsgAdmin { get; set; } = true;

                [JsonProperty(PropertyName = "Отправлять игрокам сообщение о вайпе автоматически?")]
                [DefaultValue(false)]
                public bool WMsgPlayers { get; set; } = false;

                [JsonProperty(PropertyName = "Текст сообщения игрокам о вайпе (сообщение отправляется только тем кто подписался командой /vk wipealerts)")]
                [DefaultValue("Сервер вайпнут! Залетай скорее!")]
                public string WMsgText { get; set; } = "Сервер вайпнут! Залетай скорее!";

                [JsonProperty(PropertyName = "Игнорировать команду /vk wipealerts? (если включено, сообщение о вайпе будет отправляться всем)")]
                [DefaultValue(false)]
                public bool WCMDIgnore { get; set; } = false;

                [JsonProperty(PropertyName = "Смена названия группы после вайпа")]
                [DefaultValue(false)]
                public bool GrNameChange { get; set; } = false;

                [JsonProperty(PropertyName = "Название группы (переменная {wipedate} отображает дату последнего вайпа)")]
                [DefaultValue("ServerName | WIPE {wipedate}")]
                public string GrName { get; set; } = "ServerName | WIPE {wipedate}";
            }

            public class GroupGifts
            {
                [JsonProperty(PropertyName = "Выдавать подарок игроку за вступление в группу ВК?")]
                [DefaultValue(true)]
                public bool VKGroupGifts { get; set; } = true;

                [JsonProperty(PropertyName = "Подарки за вступление в группу (shortname предмета, количество)")]
                [DefaultValue(null)]
                public Dictionary<string, object> VKGroupGiftList { get; set; } = new Dictionary<string, object>
                {
                  {"supply.signal", 1},
                  {"pookie.bear", 2}
                };

                [JsonProperty(PropertyName = "Подарок за вступление в группу (команда, если стоит none выдаются предметы из списка выше). Пример: grantperm {steamid} vkraidalert.allow 7d")]
                [DefaultValue("none")]
                public string VKGroupGiftCMD { get; set; } = "none";

                [JsonProperty(PropertyName = "Описание команды")]
                [DefaultValue("Оповещения о рейде на 7 дней")]
                public string GiftCMDdesc { get; set; } = "Оповещения о рейде на 7 дней";

                [JsonProperty(PropertyName = "Ссылка на группу ВК")]
                [DefaultValue("vk.com/1234")]
                public string VKGroupUrl { get; set; } = "vk.com/1234";

                [JsonProperty(PropertyName = "Оповещения в общий чат о получении награды")]
                [DefaultValue(true)]
                public bool GiftsBool { get; set; } = true;

                [JsonProperty(PropertyName = "Включить оповещения для игроков не получивших награду за вступление в группу?")]
                [DefaultValue(true)]
                public bool VKGGNotify { get; set; } = true;

                [JsonProperty(PropertyName = "Интервал оповещений для игроков не получивших награду за вступление в группу (в минутах)")]
                [DefaultValue(30)]
                public int VKGGTimer { get; set; } = 30;

                [JsonProperty(PropertyName = "Выдавать награду каждый вайп?")]
                [DefaultValue(true)]
                public bool GiftsWipe { get; set; } = true;
            }

            public class BDayGiftSet
            {
                [JsonProperty(PropertyName = "Включить награду для именинников?")]
                [DefaultValue(true)]
                public bool BDayEnabled { get; set; } = true;

                [JsonProperty(PropertyName = "Группа для именинников")]
                [DefaultValue("bdaygroup")]
                public string BDayGroup { get; set; } = "bdaygroup";

                [JsonProperty(PropertyName = "Оповещения в общий чат о имениннках")]
                [DefaultValue(false)]
                public bool BDayNotify { get; set; } = false;
            }

            public class MultipleServersSettings
            {
                [JsonProperty(PropertyName = "Включить поддержку несколько серверов?")]
                [DefaultValue(false)]
                public bool MSSEnable { get; set; } = false;

                [JsonProperty(PropertyName = "Номер сервера")]
                [DefaultValue(1)]
                public int ServerNumber { get; set; } = 1;

                [JsonProperty(PropertyName = "Сервер 1 IP:PORT (пример: 111.111.111.111:28015)")]
                [DefaultValue("none")]
                public string Server1ip { get; set; } = "none";

                [JsonProperty(PropertyName = "Сервер 2 IP:PORT (пример: 111.111.111.111:28015)")]
                [DefaultValue("none")]
                public string Server2ip { get; set; } = "none";

                [JsonProperty(PropertyName = "Сервер 3 IP:PORT (пример: 111.111.111.111:28015)")]
                [DefaultValue("none")]
                public string Server3ip { get; set; } = "none";

                [JsonProperty(PropertyName = "Сервер 4 IP:PORT (пример: 111.111.111.111:28015)")]
                [DefaultValue("none")]
                public string Server4ip { get; set; } = "none";

                [JsonProperty(PropertyName = "Сервер 5 IP:PORT (пример: 111.111.111.111:28015)")]
                [DefaultValue("none")]
                public string Server5ip { get; set; } = "none";

                [JsonProperty(PropertyName = "Онлайн в emoji?")]
                [DefaultValue(true)]
                public bool EmojiStatus { get; set; } = true;
            }

            public class PlayersCheckingSettings
            {
                [JsonProperty(PropertyName = "Текст уведомления")]
                [DefaultValue("<color=#990404>Модератор вызвал вас на проверку.</color> \nНапишите свой скайп с помощью команды <color=#990404>/skype <НИК в СКАЙПЕ>.</color>\nЕсли вы покините сервер, Вы будете забанены на нашем проекте серверов.")]
                public string PlCheckText { get; set; } = "<color=#990404>Модератор вызвал вас на проверку.</color> \nНапишите свой скайп с помощью команды <color=#990404>/skype <НИК в СКАЙПЕ>.</color>\nЕсли вы покините сервер, Вы будете забанены на нашем проекте серверов.";

                [JsonProperty(PropertyName = "Размер текста")]
                [DefaultValue(17)]
                public int PlCheckSize { get; set; } = 17;

                [JsonProperty(PropertyName = "Привилегия для команд /alert и /unalert")]
                [DefaultValue("vkbot.checkplayers")]
                public string PlCheckPerm { get; set; } = "vkbot.checkplayers";

                [JsonProperty(PropertyName = "Бан игрока при выходе с сервера во время проверки")]
                [DefaultValue(false)]
                public bool AutoBan { get; set; } = false;

                [JsonProperty(PropertyName = "Позиция GUI AnchorMin (дефолт 0 0.826)")]
                [DefaultValue("0 0.826")]
                public string GUIAnchorMin { get; set; } = "0 0.826";

                [JsonProperty(PropertyName = "Позиция GUI AnchorMax (дефолт 1 0.965)")]
                [DefaultValue("1 0.965")]
                public string GUIAnchorMax { get; set; } = "1 0.965";
            }

            public class TopWPlPromoSet
            {
                [JsonProperty(PropertyName = "Включить топ игроков вайпа")]
                [DefaultValue(true)]
                public bool TopWPlEnabled { get; set; } = true;

                [JsonProperty(PropertyName = "Включить отправку промо кодов за топ?")]
                [DefaultValue(false)]
                public bool TopPlPromoGift { get; set; } = false;

                [JsonProperty(PropertyName = "Пост на стене группы о топ игроках вайпа")]
                [DefaultValue(true)]
                public bool TopPlPost { get; set; } = true;

                [JsonProperty(PropertyName = "Ссылка на изображение к посту вида 'photo-1_265827614' (изображение должно быть в альбоме группы), оставить 'none' если не нужно")]
                [DefaultValue("none")]
                public string TopPlPostAtt { get; set; } = "none";

                [JsonProperty(PropertyName = "Промо для топ рэйдера")]
                [DefaultValue("topraider")]
                public string TopRaiderPromo { get; set; } = "topraider";

                [JsonProperty(PropertyName = "Ссылка на изображение к сообщению топ рейдеру вида 'photo-1_265827614' (изображение должно быть в альбоме группы), оставить 'none' если не нужно")]
                [DefaultValue("none")]
                public string TopRaiderPromoAtt { get; set; } = "none";

                [JsonProperty(PropertyName = "Промо для топ килера")]
                [DefaultValue("topkiller")]
                public string TopKillerPromo { get; set; } = "topkiller";

                [JsonProperty(PropertyName = "Ссылка на изображение к сообщению топ киллеру вида 'photo-1_265827614' (изображение должно быть в альбоме группы), оставить 'none' если не нужно")]
                [DefaultValue("none")]
                public string TopKillerPromoAtt { get; set; } = "none";

                [JsonProperty(PropertyName = "Промо для топ фармера")]
                [DefaultValue("topfarmer")]
                public string TopFarmerPromo { get; set; } = "topfarmer";

                [JsonProperty(PropertyName = "Ссылка на изображение к сообщению топ фармеру вида 'photo-1_265827614' (изображение должно быть в альбоме группы), оставить 'none' если не нужно")]
                [DefaultValue("none")]
                public string TopFarmerPromoAtt { get; set; } = "none";

                [JsonProperty(PropertyName = "Ссылка на донат магазин")]
                [DefaultValue("server.gamestores.ru")]
                public string StoreUrl { get; set; } = "server.gamestores.ru";

                [JsonProperty(PropertyName = "Автоматическая генерация промокодов после вайпа")]
                [DefaultValue(false)]
                public bool GenRandomPromo { get; set; } = false;
            }

            public class CommandSettings
            {
                [JsonProperty(PropertyName = "Команда отправки сообщения администратору")]
                [DefaultValue("report")]
                public string CMDreport { get; set; } = "report";

                [JsonProperty(PropertyName = "Команда вызова игрока на проверку")]
                [DefaultValue("alert")]
                public string CMDalert { get; set; } = "alert";

                [JsonProperty(PropertyName = "Команда завершения проверки игрока")]
                [DefaultValue("unalert")]
                public string CMDunalert { get; set; } = "unalert";

                [JsonProperty(PropertyName = "Команда отправки скайпа модератору")]
                [DefaultValue("skype")]
                public string CMDskype { get; set; } = "skype";
            }

            public class DynamicGroupLabelSettings
            {
                [JsonProperty(PropertyName = "Включить динамическую обложку?")]
                [DefaultValue(false)]
                public bool DLEnable { get; set; } = false;

                [JsonProperty(PropertyName = "Ссылка на скрипт обновления")]
                [DefaultValue("none")]
                public string DLUrl { get; set; } = "none";

                [JsonProperty(PropertyName = "Таймер обновления (в минутах)")]
                [DefaultValue(10)]
                public int DLTimer { get; set; } = 10;

                [JsonProperty(PropertyName = "Обложка с онлайном нескольких серверов (все настройки ниже игнорируются)")]
                [DefaultValue(false)]
                public bool DLMSEnable { get; set; } = false;

                [JsonProperty(PropertyName = "Текст блока 1 (доступны все переменные как в статусе)")]
                [DefaultValue("none")]
                public string DLText1 { get; set; } = "none";

                [JsonProperty(PropertyName = "Текст блока 2 (доступны все переменные как в статусе)")]
                [DefaultValue("none")]
                public string DLText2 { get; set; } = "none";

                [JsonProperty(PropertyName = "Текст блока 3 (доступны все переменные как в статусе)")]
                [DefaultValue("none")]
                public string DLText3 { get; set; } = "none";

                [JsonProperty(PropertyName = "Текст блока 4 (доступны все переменные как в статусе)")]
                [DefaultValue("none")]
                public string DLText4 { get; set; } = "none";

                [JsonProperty(PropertyName = "Текст блока 5 (доступны все переменные как в статусе)")]
                [DefaultValue("none")]
                public string DLText5 { get; set; } = "none";

                [JsonProperty(PropertyName = "Текст блока 6 (доступны все переменные как в статусе)")]
                [DefaultValue("none")]
                public string DLText6 { get; set; } = "none";

                [JsonProperty(PropertyName = "Текст блока 7 (доступны все переменные как в статусе)")]
                [DefaultValue("none")]
                public string DLText7 { get; set; } = "none";
            }
        }
        private void LoadVariables()
        {
            bool changed = false;
            Config.Settings.DefaultValueHandling = DefaultValueHandling.Populate;
            config = Config.ReadObject<ConfigData>();
            if (config.AdmNotify == null)
            {
                config.AdmNotify = new ConfigData.AdminNotify();
                changed = true;
            }
            if (config.ChNotify == null)
            {
                config.ChNotify = new ConfigData.ChatNotify();
                changed = true;
            }
            if (config.WipeStg == null)
            {
                config.WipeStg = new ConfigData.WipeSettings();
                changed = true;
            }
            if (config.GrGifts == null)
            {
                config.GrGifts = new ConfigData.GroupGifts();
                changed = true;
            }
            if (config.TopWPlayersPromo == null)
            {
                config.TopWPlayersPromo = new ConfigData.TopWPlPromoSet();
                changed = true;
            }
            if (config.CMDSet == null)
            {
                config.CMDSet = new ConfigData.CommandSettings();
                changed = true;
            }
            if (config.DGLSet == null)
            {
                config.DGLSet = new ConfigData.DynamicGroupLabelSettings();
                changed = true;
            }
            Config.WriteObject(config, true);
            if (changed) PrintWarning("Конфигурационный файл обновлен. Добавлены новые настройки. Список изменений в плагине - vk.com/topic-30818042_36264027");
        }
        protected override void LoadDefaultConfig()
        {
            var configData = new ConfigData
            {
                VKAPIT = new ConfigData.VKAPITokens(),
                AdmNotify = new ConfigData.AdminNotify(),
                ChNotify = new ConfigData.ChatNotify(),
                StatusStg = new ConfigData.StatusSettings(),
                WipeStg = new ConfigData.WipeSettings(),
                GrGifts = new ConfigData.GroupGifts(),
                BDayGift = new ConfigData.BDayGiftSet(),
                MltServSet = new ConfigData.MultipleServersSettings(),
                PlChkSet = new ConfigData.PlayersCheckingSettings(),
                TopWPlayersPromo = new ConfigData.TopWPlPromoSet(),
                CMDSet = new ConfigData.CommandSettings(),
                DGLSet = new ConfigData.DynamicGroupLabelSettings()
            };
            Config.WriteObject(configData, true);
            PrintWarning("Поддержи разработчика! Вступи в группу vk.com/vkbotrust");
            PrintWarning("Инструкция по настройке плагина - goo.gl/xRkEUa");
        }
        #endregion

        #region Datastorage
        class DataStorageStats
        {
            public int WoodGath;
            public int SulfureGath;
            public int Rockets;
            public int Blueprints;
            public int Explosive;
            public int Reports;
            public DataStorageStats() { }
        }
        class DataStorageUsers
        {
            public Dictionary<ulong, VKUDATA> VKUsersData = new Dictionary<ulong, VKUDATA>();
            public DataStorageUsers() { }
        }
        class VKUDATA
        {
            public ulong UserID;
            public string Name;
            public string VkID;
            public int ConfirmCode;
            public bool Confirmed;
            public bool GiftRecived;
            public string LastRaidNotice;
            public bool WipeMsg;
            public string Bdate;
            public int Raids;
            public int Kills;
            public int Farm;
        }
        class DataStorageReports
        {
            public Dictionary<int, REPORT> VKReportsData = new Dictionary<int, REPORT>();
            public DataStorageReports() { }
        }
        class REPORT
        {
            public ulong UserID;
            public string Name;
            public string Text;
        }
        DataStorageStats statdata;
        DataStorageUsers usersdata;
        DataStorageReports reportsdata;
        private DynamicConfigFile VKBData;
        private DynamicConfigFile StatData;
        private DynamicConfigFile ReportsData;
        void LoadData()
        {
            try
            {
                statdata = Interface.GetMod().DataFileSystem.ReadObject<DataStorageStats>("VKBot");
                usersdata = Interface.GetMod().DataFileSystem.ReadObject<DataStorageUsers>("VKBotUsers");
                reportsdata = Interface.GetMod().DataFileSystem.ReadObject<DataStorageReports>("VKBotReports");
            }

            catch
            {
                statdata = new DataStorageStats();
                usersdata = new DataStorageUsers();
                reportsdata = new DataStorageReports();
            }
        }
        #endregion

        #region Oxidehooks
        void OnServerInitialized()
        {
            LoadVariables();
            VKBData = Interface.Oxide.DataFileSystem.GetFile("VKBotUsers");
            StatData = Interface.Oxide.DataFileSystem.GetFile("VKBot");
            ReportsData = Interface.Oxide.DataFileSystem.GetFile("VKBotReports");
            LoadData();
            cmd.AddChatCommand(config.CMDSet.CMDalert, this, "StartCheckPlayer");
            cmd.AddChatCommand(config.CMDSet.CMDunalert, this, "StopCheckPlayer");
            cmd.AddChatCommand(config.CMDSet.CMDskype, this, "SkypeSending");
            cmd.AddChatCommand(config.CMDSet.CMDreport, this, "SendReport");
            CheckAdminID();
            if (!permission.PermissionExists(config.PlChkSet.PlCheckPerm)) permission.RegisterPermission(config.PlChkSet.PlCheckPerm, this);
            if (NewWipe)
            {
                WipeFunctions(mapfile);
            }
            if (config.StatusStg.UpdateStatus)
            {
                if (config.StatusStg.StatusSet == 1)
                {
                    timer.Repeat(config.StatusStg.UpdateTimer * 60, 0, Update1ServerStatus);
                }
                if (config.StatusStg.StatusSet == 2)
                {
                    timer.Repeat(config.StatusStg.UpdateTimer * 60, 0, () => { UpdateMultiServerStatus("status"); });
                }
            }
            if (config.DGLSet.DLEnable && config.DGLSet.DLUrl != "none")
            {
                timer.Repeat(config.DGLSet.DLTimer * 60, 0, () => {
                    if (config.DGLSet.DLMSEnable)
                    {
                        UpdateMultiServerStatus("label");
                    }
                    else
                    {
                        UpdateVKLabel();
                    }
                });
            }
            if (config.GrGifts.VKGGNotify)
            {
                timer.Repeat(config.GrGifts.VKGGTimer * 60, 0, GiftNotifier);
            }
            if (config.AdmNotify.PluginsCheckMsg)
            {
                CheckPlugins();
            }
        }
        void OnServerSave()
        {
            if (config.TopWPlayersPromo.TopWPlEnabled)
            {
                VKBData.WriteObject(usersdata);
            }
        }
        private void Init()
        {            
            cmd.AddChatCommand("vk", this, "VKcommand");
            cmd.AddConsoleCommand("updatestatus", this, "UStatus");
            cmd.AddConsoleCommand("updatelabel", this, "ULabel");
            cmd.AddConsoleCommand("sendmsgadmin", this, "MsgAdmin");
            cmd.AddConsoleCommand("wipealerts", this, "WipeAlerts");            
            cmd.AddConsoleCommand("userinfo", this, "GetUserInfo");
            cmd.AddConsoleCommand("report.answer", this, "ReportAnswer");
            cmd.AddConsoleCommand("report.list", this, "ReportList");
            cmd.AddConsoleCommand("report.wipe", this, "ReportClear");
            jsonsettings = new JsonSerializerSettings();
            jsonsettings.Converters.Add(new KeyValuePairConverter());
        }
        void Loaded()
        {
            LoadMessages();
        }
        void Unload()
        {
            if (config.AdmNotify.SendReports)
            {
                ReportsData.WriteObject(reportsdata);
            }
            if (config.StatusStg.UpdateStatus)
            {
                StatData.WriteObject(statdata);
            }
            if (config.TopWPlayersPromo.TopWPlEnabled)
            {
                VKBData.WriteObject(usersdata);
            }
            if (PlayersCheckList.Count != 0)
            {
                foreach (var player in PlayersCheckList)
                {
                    var target = BasePlayer.FindByID(player.Value);
                    if (target != null) StopGui(target);
                }
                PlayersCheckList.Clear();                
            }            
            if (config.BDayGift.BDayEnabled && BDayPlayers.Count > 0)
            {
                foreach (var id in BDayPlayers)
                {
                    permission.RemoveUserGroup(id.ToString(), config.BDayGift.BDayGroup);
                }
                BDayPlayers.Clear();
            }
            if (ConfCods.Count != 0)
            {
                foreach (var pl in ConfCods)
                {
                    CuiHelper.DestroyUi(pl, "VKConfGUI");
                }
                ConfCods.Clear();
            }
        }
        void OnNewSave(string filename)
        {
            NewWipe = true;
            mapfile = filename;
        }
        void OnPlayerInit(BasePlayer player)
        {
            if (usersdata.VKUsersData.ContainsKey(player.userID))
            {
                if (usersdata.VKUsersData[player.userID].Name != player.displayName)
                {
                    usersdata.VKUsersData[player.userID].Name = player.displayName;
                    VKBData.WriteObject(usersdata);
                }
                if (usersdata.VKUsersData[player.userID].Bdate == null)
                {
                    AddBdate(player);
                }
            }
        }
        void OnPlayerSleepEnded(BasePlayer player)
        {
            if (!usersdata.VKUsersData.ContainsKey(player.userID)) return;
            if (!config.BDayGift.BDayEnabled) return;
            if (config.BDayGift.BDayEnabled && permission.GroupExists(config.BDayGift.BDayGroup))
            {
                if (permission.UserHasGroup(player.userID.ToString(), config.BDayGift.BDayGroup)) return;
                var today = DateTime.Now.ToString("d.M", CultureInfo.InvariantCulture);
                var bday = usersdata.VKUsersData[player.userID].Bdate;
                if (bday == null || bday == "noinfo") return;
                string[] array = bday.Split('.');
                if (array.Length == 3)
                {
                    bday.Remove(bday.Length - 5, 5);
                }
                if (bday == today)
                {
                    permission.AddUserGroup(player.userID.ToString(), config.BDayGift.BDayGroup);
                    PrintToChat(player, string.Format(GetMsg("ПоздравлениеИгрока", player)));
                    Log("bday", $"Игрок {player.displayName} добавлен в группу {config.BDayGift.BDayGroup}");
                    BDayPlayers.Add(player.userID);
                    if (config.BDayGift.BDayNotify)
                    {
                        Server.Broadcast(string.Format(GetMsg("ДеньРожденияИгрока", player), player.displayName));
                    }
                }
            }
        }
        void OnPlayerDisconnected(BasePlayer player, string reason)
        {
            if (PlayersCheckList.Count != 0)
            {
                if (PlayersCheckList.ContainsValue(player.userID))
                {
                    CuiHelper.DestroyUi(player, "AlertGUI");
                    BasePlayer moder = null;
                    for (int i = 0; i < PlayersCheckList.Count; i++)
                    {
                        if (PlayersCheckList.ElementAt(i).Value == player.userID)
                        {
                            ulong moderid = PlayersCheckList.ElementAt(i).Key;
                            moder = BasePlayer.FindByID(moderid);
                            PlayersCheckList.Remove(moderid);
                            if (moder != null)
                            {
                                PrintToChat(moder, $"Игрок вызванный на проверку покинул сервер. Причина: {reason}");
                            }
                        }
                    }
                    if (config.PlChkSet.AutoBan || reason == "Disconnected")
                    {
                        player.IPlayer.Ban("Отказ от проверки");
                        if (player.IsConnected)
                        {
                            player.Kick("banned");
                        }
                    }
                }
                if (PlayersCheckList.ContainsKey(player.userID))
                {
                    ulong targetid;
                    PlayersCheckList.TryGetValue(player.userID, out targetid);
                    BasePlayer target = BasePlayer.FindByID(targetid);
                    if (target != null)
                    {
                        CuiHelper.DestroyUi(target, "AlertGUI");
                        PrintToChat(target, string.Format(GetMsg("МодераторОтключился", player)));
                    }
                    PlayersCheckList.Remove(player.userID);
                }
            }            
            if (config.BDayGift.BDayEnabled && permission.GroupExists(config.BDayGift.BDayGroup))
            {
                if (BDayPlayers.Contains(player.userID))
                {
                    permission.RemoveUserGroup(player.userID.ToString(), config.BDayGift.BDayGroup);
                    BDayPlayers.Remove(player.userID);
                    Log("bday", $"Игрок {player.displayName} удален из группы {config.BDayGift.BDayGroup}");
                }
            }
            if (ConfCods.Count != 0 && ConfCods.Contains(player))
            {
                CuiHelper.DestroyUi(player, "VKConfGUI");
                ConfCods.Remove(player);
            }
        }
        #endregion

        #region Stats
        private void OnItemResearch(ResearchTable table, Item targetItem, BasePlayer player)
        {
            if (config.StatusStg.UpdateStatus || config.DGLSet.DLEnable)
            {
                statdata.Blueprints++;
            }
        }
        private void OnDispenserGather(ResourceDispenser dispenser, BaseEntity entity, Item item)
        {
            if (config.StatusStg.UpdateStatus || config.DGLSet.DLEnable)
            {
                if (item.info.shortname == "wood")
                {
                    statdata.WoodGath = statdata.WoodGath + item.amount;
                }
                if (item.info.shortname == "sulfur.ore")
                {
                    statdata.SulfureGath = statdata.SulfureGath + item.amount;
                }
            }
            if (config.TopWPlayersPromo.TopWPlEnabled)
            {
                BasePlayer player = entity.ToPlayer();
                if (player == null) return;
                if (usersdata.VKUsersData.ContainsKey(player.userID))
                {
                    usersdata.VKUsersData[player.userID].Farm = usersdata.VKUsersData[player.userID].Farm + item.amount;
                }
            }
        }
        private void OnDispenserBonus(ResourceDispenser dispenser, BasePlayer player, Item item)
        {
            if ((config.StatusStg.UpdateStatus || config.DGLSet.DLEnable) && item.info.shortname == "sulfur.ore")
            {
                statdata.SulfureGath = statdata.SulfureGath + item.amount;
            }
            if (config.TopWPlayersPromo.TopWPlEnabled)
            {
                if (usersdata.VKUsersData.ContainsKey(player.userID))
                {
                    usersdata.VKUsersData[player.userID].Farm = usersdata.VKUsersData[player.userID].Farm + item.amount;
                }
            }
        }
        private void OnCollectiblePickup(Item item, BasePlayer player)
        {
            if (config.StatusStg.UpdateStatus || config.DGLSet.DLEnable)
            {
                if (item.info.shortname == "wood")
                {
                    statdata.WoodGath = statdata.WoodGath + item.amount;
                }
                if (item.info.shortname == "sulfur.ore")
                {
                    statdata.SulfureGath = statdata.SulfureGath + item.amount;
                }
            }
            if (config.TopWPlayersPromo.TopWPlEnabled)
            {
                if (usersdata.VKUsersData.ContainsKey(player.userID))
                {
                    usersdata.VKUsersData[player.userID].Farm = usersdata.VKUsersData[player.userID].Farm + item.amount;
                }
            }
        }
        private void OnRocketLaunched(BasePlayer player, BaseEntity entity)
        {
            if (config.StatusStg.UpdateStatus || config.DGLSet.DLEnable)
            {
                statdata.Rockets++;
            }
        }
        private void OnExplosiveThrown(BasePlayer player, BaseEntity entity)
        {
            if (config.StatusStg.UpdateStatus || config.DGLSet.DLEnable)
            {
                List<object> include = new List<object>()
                {
                "explosive.satchel.deployed",
                "grenade.f1.deployed",
                "grenade.beancan.deployed",
                "explosive.timed.deployed"
                };
                if (include.Contains(entity.ShortPrefabName))
                {
                    statdata.Explosive++;
                }
            }
        }
        void OnEntityDeath(BaseCombatEntity entity, HitInfo hitInfo)
        {
            if (config.TopWPlayersPromo.TopWPlEnabled)
            {
                if (entity.name.Contains("corpse"))  return;
                if (hitInfo == null) return;
                var attacker = hitInfo.Initiator?.ToPlayer();
                if (attacker == null) return;
                if (entity is BasePlayer)
                {
                    CheckDeath(entity.ToPlayer(), hitInfo, attacker);
                }
                if (entity is BaseEntity)
                {
                    if (hitInfo.damageTypes.GetMajorityDamageType() != Rust.DamageType.Explosion && hitInfo.damageTypes.GetMajorityDamageType() != Rust.DamageType.Heat && hitInfo.damageTypes.GetMajorityDamageType() != Rust.DamageType.Bullet) return;
                    if (attacker.userID == entity.OwnerID) return;
                    BuildingBlock block = entity.GetComponent<BuildingBlock>();
                    if (block != null)
                    {
                        if (block.currentGrade.gradeBase.type.ToString() == "Twigs" || block.currentGrade.gradeBase.type.ToString() == "Wood")
                        {
                            return;
                        }
                    }
                    else
                    {
                        bool ok = false;
                        foreach (var ent in allowedentity)
                        {
                            if (entity.LookupPrefab().name.Contains(ent))
                            {
                                ok = true;
                            }
                        }
                        if (!ok) return;
                    }
                    if (entity.OwnerID == 0) return;
                    if (usersdata.VKUsersData.ContainsKey(attacker.userID))
                    {
                        usersdata.VKUsersData[attacker.userID].Raids++;
                    }
                }                
            }
        }
        private void CheckDeath(BasePlayer player, HitInfo info, BasePlayer attacker)
        {
            if (IsNPC(player)) return;
            if (!usersdata.VKUsersData.ContainsKey(attacker.userID)) return;
            if (!player.IsConnected) return;
            bool Duelist = false;
            if (plugins.Exists("Duel"))
            {
                Duelist = (bool)Duel?.Call("IsDuelPlayer", player);
            }            
            if (Duelist) return;
            usersdata.VKUsersData[attacker.userID].Kills++;
        }
        #endregion

        #region Wipe
        private void WipeFunctions(string filename)
        {
            if (config.StatusStg.UpdateStatus)
            {
                statdata.Blueprints = 0;
                statdata.Rockets = 0;
                statdata.SulfureGath = 0;
                statdata.WoodGath = 0;
                statdata.Explosive = 0;
                StatData.WriteObject(statdata);
                NewWipe = false;
                if (config.StatusStg.StatusSet == 1) { Update1ServerStatus(); }
                if (config.StatusStg.StatusSet == 2) { UpdateMultiServerStatus("status"); }
            }
            if (config.WipeStg.WPostMsgAdmin)
            {
                string s = filename;
                string[] array = s.Split('/');
                int t = array.Length - 1;
                string savename = array[t];
                string[] mapname = savename.Split('.');
                string msg2 = null;
                if (config.MltServSet.MSSEnable)
                {
                    msg2 = $"[VKBot] Сервер {config.MltServSet.ServerNumber.ToString()} вайпнут. Установлена карта: {mapname[0]}. Размер: {mapname[1]}. Сид: {mapname[2]}";
                }
                else
                {
                    msg2 = $"[VKBot] Сервер вайпнут. Установлена карта: {mapname[0]}. Размер: {mapname[1]}. Сид: {mapname[2]}";
                }
                if (config.ChNotify.ChNotfEnabled && config.ChNotify.ChNotfSet.Contains("wipe"))
                {
                    SendChatMessage(config.ChNotify.ChatID, msg2);
                    if (config.ChNotify.AdmMsg) SendVkMessage(config.AdmNotify.VkID, msg2);
                }
                else
                {
                    SendVkMessage(config.AdmNotify.VkID, msg2);
                }
            }
            if (config.WipeStg.WPostB)
            {
                if (config.WipeStg.WPostAttB)
                {
                    SendVkWall($"{config.WipeStg.WPostMsg}&attachments={config.WipeStg.WPostAtt}");
                }
                else
                {
                    SendVkWall($"{config.WipeStg.WPostMsg}");
                }
            }
            if (config.GrGifts.GiftsWipe)
            {
                int amount = usersdata.VKUsersData.Count;
                if (amount != 0)
                {
                    for (int i = 0; i < amount; i++)
                    {
                        usersdata.VKUsersData.ElementAt(i).Value.GiftRecived = false;
                    }
                    VKBData.WriteObject(usersdata);
                }
            }
            if (config.TopWPlayersPromo.TopWPlEnabled)
            {
                if (config.TopWPlayersPromo.TopPlPost || config.TopWPlayersPromo.TopPlPromoGift)
                {
                    SendPromoMsgsAndPost();
                    if (config.TopWPlayersPromo.TopPlPromoGift && config.TopWPlayersPromo.GenRandomPromo)
                    {
                        SetRandomPromo();
                    }
                }
                int amount = usersdata.VKUsersData.Count;
                if (amount != 0)
                {
                    for (int i = 0; i < amount; i++)
                    {
                        usersdata.VKUsersData.ElementAt(i).Value.Farm = 0;
                        usersdata.VKUsersData.ElementAt(i).Value.Kills = 0;
                        usersdata.VKUsersData.ElementAt(i).Value.Raids = 0;
                    }
                    VKBData.WriteObject(usersdata);
                }
            }
            if (config.WipeStg.WMsgPlayers)
            {
                WipeAlertsSend();
            }
            if (config.AdmNotify.SendReports && config.AdmNotify.ReportsWipe)
            {
                reportsdata.VKReportsData.Clear();
                ReportsData.WriteObject(reportsdata);
                statdata.Reports = 0;
                StatData.WriteObject(statdata);
            }
            if (config.WipeStg.GrNameChange)
            {
                string wipedate = WipeDate();
                string text = config.WipeStg.GrName.Replace("{wipedate}", wipedate);
                string url = "https://api.vk.com/method/groups.edit?group_id=" + config.VKAPIT.GroupID + "&title=" + text + "&v=5.71&access_token=" + config.VKAPIT.VKTokenApp;
                webrequest.Enqueue(url, null, (code, response) => 
                {
                    var json = JObject.Parse(response);
                    string Result = (string)json["response"];
                    if (Result == "1")
                    {
                        PrintWarning($"Новое имя группы - {text}");
                    }
                    else
                    {
                        PrintWarning("Ошибка смены имени группы. Логи - /oxide/logs/VKBot/");
                        Log("Errors", $"group title not changed. Error: {response}");
                    }
                }, this);
            }
        }
        private void WipeAlerts(ConsoleSystem.Arg arg)
        {
            if (arg.IsAdmin != true) { return; }
            WipeAlertsSend();
        }
        private void WipeAlertsSend()
        {
            List<string> UserList = new List<string>();
            var BannedUsers = ServerUsers.BanListString();
            string userlist = "";
            int usercount = 0;
            int amount = usersdata.VKUsersData.Count;
            if (amount != 0)
            {
                for (int i = 0; i < amount; i++)
                {
                    if (config.WipeStg.WCMDIgnore || usersdata.VKUsersData.ElementAt(i).Value.WipeMsg)
                    {
                        if (!BannedUsers.Contains(usersdata.VKUsersData.ElementAt(i).Value.UserID.ToString()))
                        {
                            if (usercount == 100)
                            {
                                UserList.Add(userlist);
                                userlist = "";
                                usercount = 0;
                            }
                            if (usercount > 0)
                            {
                                userlist = userlist + ", ";
                            }
                            userlist = userlist + usersdata.VKUsersData.ElementAt(i).Value.VkID;
                            usercount++;
                        }                        
                    }
                }
            }
            if (userlist == "" && UserList.Count == 0) { PrintWarning($"Список адресатов рассылки о вайпе пуст."); return; }
            if (UserList.Count > 0)
            {
                foreach (var list in UserList)
                {
                    SendVkMessage(list, config.WipeStg.WMsgText);
                }
            }
            SendVkMessage(userlist, config.WipeStg.WMsgText);
        }
        #endregion

        #region MainMethods
        private void UStatus(ConsoleSystem.Arg arg)
        {
            if (arg.IsAdmin != true) { return; }
            if (config.StatusStg.UpdateStatus)
            {
                if (config.StatusStg.StatusSet == 1) { Update1ServerStatus(); }
                if (config.StatusStg.StatusSet == 2) { UpdateMultiServerStatus("status"); }
            }
            else
            {
                PrintWarning($"Функция обновления статуса отключена.");
            }
        }
        private string PrepareStatus(string input, string target)
        {
            string text = input;
            string temp = "";

            temp = GetOnline();
            if (target == "status" && config.StatusStg.EmojiCounterList.Contains("onlinecounter")) temp = EmojiCounters(temp);
            if (input.Contains("{onlinecounter}")) text = text.Replace("{onlinecounter}", temp);

            temp = BasePlayer.sleepingPlayerList.Count.ToString();
            if (target == "status" && config.StatusStg.EmojiCounterList.Contains("sleepers")) temp = EmojiCounters(temp);
            if (input.Contains("{sleepers}")) text = text.Replace("{sleepers}", temp);

            temp = statdata.WoodGath.ToString();
            if (target == "status" && config.StatusStg.EmojiCounterList.Contains("woodcounter")) temp = EmojiCounters(temp);
            if (input.Contains("{woodcounter}")) text = text.Replace("{woodcounter}", temp);

            temp = statdata.SulfureGath.ToString();
            if (target == "status" && config.StatusStg.EmojiCounterList.Contains("sulfurecounter")) temp = EmojiCounters(temp);
            if (input.Contains("{sulfurecounter}")) text = text.Replace("{sulfurecounter}", temp);

            temp = statdata.Rockets.ToString();
            if (target == "status" && config.StatusStg.EmojiCounterList.Contains("rocketscounter")) temp = EmojiCounters(temp);
            if (input.Contains("{rocketscounter}")) text = text.Replace("{rocketscounter}", temp);

            temp = statdata.Blueprints.ToString();
            if (target == "status" && config.StatusStg.EmojiCounterList.Contains("blueprintsconter")) temp = EmojiCounters(temp);
            if (input.Contains("{blueprintsconter}")) text = text.Replace("{blueprintsconter}", temp);

            temp = statdata.Explosive.ToString();
            if (target == "status" && config.StatusStg.EmojiCounterList.Contains("explosivecounter")) temp = EmojiCounters(temp);
            if (input.Contains("{explosivecounter}")) text = text.Replace("{explosivecounter}", temp);

            temp = (string)WipeDate();
            if (target == "status" && config.StatusStg.EmojiCounterList.Contains("wipedate")) temp = EmojiCounters(temp);
            if (input.Contains("{wipedate}")) text = text.Replace("{wipedate}", temp);

            temp = config.StatusStg.connecturl;
            if (target == "status" && config.StatusStg.EmojiCounterList.Contains("connect")) temp = EmojiCounters(temp);
            if (input.Contains("{connect}")) text = text.Replace("{connect}", temp);

            temp = config.StatusStg.StatusUT;
            if (target == "status" && config.StatusStg.EmojiCounterList.Contains("usertext")) temp = EmojiCounters(temp);
            if (input.Contains("{usertext}")) text = text.Replace("{usertext}", temp);

            temp = DateTime.Now.ToString("HH:mm", CultureInfo.InvariantCulture);
            if (target == "status" && config.StatusStg.EmojiCounterList.Contains("updatetime")) temp = EmojiCounters(temp);
            if (input.Contains("{updatetime}")) text = text.Replace("{updatetime}", temp);
            return text;
        }
        void OnPlayerBanned(string name, ulong id, string address, string reason)
        {
            if (config.AdmNotify.UserBannedMsg)
            {
                string msg2 = null;
                if (config.MltServSet.MSSEnable)
                {
                    msg2 = $"[Сервер {config.MltServSet.ServerNumber.ToString()}] Игрок {name} ({id}) был забанен на сервере. Причина: {reason}. Ссылка на профиль стим: steamcommunity.com/profiles/{id}/";
                }
                else
                {
                    msg2 = $"Игрок {name} ({id}) был забанен на сервере. Причина: {reason}. Ссылка на профиль стим: steamcommunity.com/profiles/{id}/";
                }
                if (usersdata.VKUsersData.ContainsKey(id) && usersdata.VKUsersData[id].Confirmed)
                {
                    msg2 = msg2 + $" . Ссылка на профиль ВК: vk.com/id{usersdata.VKUsersData[id].VkID}";
                }
                if (config.ChNotify.ChNotfEnabled && config.ChNotify.ChNotfSet.Contains("bans"))
                {
                    SendChatMessage(config.ChNotify.ChatID, msg2);
                    if (config.ChNotify.AdmMsg) SendVkMessage(config.AdmNotify.VkID, msg2);
                }
                else
                {
                    SendVkMessage(config.AdmNotify.VkID, msg2);
                }
            }           
        }
        private void SendReport(BasePlayer player, string cmd, string[] args)
        {
            if (config.AdmNotify.SendReports)
            {
                if (args.Length > 0)
                {
                    string text = string.Join(" ", args.Skip(0).ToArray());
                    string reporttext = "[VKBot]";
                    statdata.Reports = statdata.Reports + 1;
                    int reportid = statdata.Reports;
                    StatData.WriteObject(statdata);
                    if (config.MltServSet.MSSEnable)
                    {
                        reporttext = reporttext + " [Сервер " + config.MltServSet.ServerNumber.ToString() + "]";
                    }
                    reporttext = reporttext + " " + player.displayName + " " + "(" + player.UserIDString + ")";
                    if (usersdata.VKUsersData.ContainsKey(player.userID))
                    {
                        if (usersdata.VKUsersData[player.userID].Confirmed)
                        {
                            reporttext = reporttext + ". ВК: vk.com/id" + usersdata.VKUsersData[player.userID].VkID;
                        }
                        else
                        {
                            reporttext = reporttext + ". ВК: vk.com/id" + usersdata.VKUsersData[player.userID].VkID + " (не подтвержден)";
                        }
                    }
                    reporttext = reporttext + " ID репорта: " + reportid + ". Сообщение: " + text; 
                    if (config.ChNotify.ChNotfEnabled && config.ChNotify.ChNotfSet.Contains("reports"))
                    {
                        SendChatMessage(config.ChNotify.ChatID, reporttext);
                        if (config.ChNotify.AdmMsg) SendVkMessage(config.AdmNotify.VkID, reporttext);
                    }
                    else
                    {
                        SendVkMessage(config.AdmNotify.VkID, reporttext);
                    }
                    reportsdata.VKReportsData.Add(reportid, new REPORT
                    {
                        UserID = player.userID,
                        Name = player.displayName,
                        Text = text
                    });
                    ReportsData.WriteObject(reportsdata);
                    Log("Log", $"{player.displayName} ({player.userID}): написал администратору: {reporttext}");
                    PrintToChat(player, string.Format(GetMsg("РепортОтправлен", player), config.AdmNotify.ReportsNotify));
                }
                else
                {
                    PrintToChat(player, string.Format(GetMsg("КомандаРепорт", player), config.AdmNotify.ReportsNotify));
                    return;
                }
            }
            else
            {
                PrintToChat(player, string.Format(GetMsg("ФункцияОтключена", player)));
            }
        }
        private void AddBdate(BasePlayer player)
        {
            if (usersdata.VKUsersData[player.userID].Bdate != null) return;
            string Userid = null;
            string userid = usersdata.VKUsersData[player.userID].VkID;
            string url2 = "https://api.vk.com/method/users.get?user_ids=" + userid + "&v=5.71&fields=bdate&access_token=" + config.VKAPIT.VKToken;
            webrequest.Enqueue(url2, null, (code, response) => {
                var json = JObject.Parse(response);
                Userid = (string)json["response"][0]["id"];
                if (Userid == null) return;
                usersdata.VKUsersData[player.userID].Bdate = "noinfo";
                var bdate = (string)json["response"][0]["bdate"];
                if (bdate != null)
                {
                    usersdata.VKUsersData[player.userID].Bdate = bdate;
                }
                VKBData.WriteObject(usersdata);
            }, this);
        }
        private void CheckVkUser(BasePlayer player, string url)
        {
            string Userid = null;
            string[] arr1 = url.Split('/');
            int num = arr1.Length - 1;
            string vkname = arr1[num];
            string url2 = "https://api.vk.com/method/users.get?user_ids=" + vkname + "&v=5.71&fields=bdate&access_token=" + config.VKAPIT.VKToken;
            webrequest.Enqueue(url2, null, (code, response) => {
                if (!response.Contains("error"))
                {
                    var json = JObject.Parse(response);
                    Userid = (string)json["response"][0]["id"];
                    string bdate = "noinfo";
                    bdate = (string)json["response"][0]["bdate"];
                    if (Userid != null)
                    {
                        AddVKUser(player, Userid, bdate);
                    }
                    else
                    {
                        PrintToChat(player, "Ошибка обработки вашей ссылки ВК, обратитесь к администратору.");
                    }
                }
            }, this);
        }
        private void AddVKUser(BasePlayer player, string Userid, string bdate)
        {
            if (!usersdata.VKUsersData.ContainsKey(player.userID))
            {
                usersdata.VKUsersData.Add(player.userID, new VKUDATA()
                {
                    UserID = player.userID,
                    Name = player.displayName,
                    VkID = Userid,
                    ConfirmCode = random.Next(1, 9999999),
                    Confirmed = false,
                    GiftRecived = false,
                    Bdate = bdate,
                    Farm = 0,
                    Kills = 0,
                    Raids = 0
                });
                VKBData.WriteObject(usersdata);
                SendConfCode(usersdata.VKUsersData[player.userID].VkID, $"Для подтверждения вашего ВК профиля введите в игровой чат команду /vk confirm {usersdata.VKUsersData[player.userID].ConfirmCode}", player);
            }
            else
            {
                if (Userid == usersdata.VKUsersData[player.userID].VkID && usersdata.VKUsersData[player.userID].Confirmed) { PrintToChat(player, string.Format(GetMsg("ПрофильДобавленИПодтвержден", player))); return; }
                if (Userid == usersdata.VKUsersData[player.userID].VkID && !usersdata.VKUsersData[player.userID].Confirmed) { PrintToChat(player, string.Format(GetMsg("ПрофильДобавлен", player))); return; }
                usersdata.VKUsersData[player.userID].Name = player.displayName;
                usersdata.VKUsersData[player.userID].VkID = Userid;
                usersdata.VKUsersData[player.userID].Confirmed = false;
                usersdata.VKUsersData[player.userID].ConfirmCode = random.Next(1, 9999999);
                usersdata.VKUsersData[player.userID].Bdate = bdate;
                VKBData.WriteObject(usersdata);
                SendConfCode(usersdata.VKUsersData[player.userID].VkID, $"Для подтверждения вашего ВК профиля введите в игровой чат команду /vk confirm {usersdata.VKUsersData[player.userID].ConfirmCode}", player);
            }
        }
        private void VKcommand(BasePlayer player, string cmd, string[] args)
        {
            if (args.Length > 0)
            {
                if (args[0] == "add")
                {
                    if (args.Length == 1) { PrintToChat(player, string.Format(GetMsg("ДоступныеКоманды", player))); return; }
                    if (!args[1].Contains("vk.com/")) { PrintToChat(player, string.Format(GetMsg("НеправильнаяСсылка", player))); return; }
                    CheckVkUser(player, args[1]);
                }
                if (args[0] == "confirm")
                {
                    if (args.Length >= 2)
                    {
                        if (usersdata.VKUsersData.ContainsKey(player.userID))
                        {
                            if (usersdata.VKUsersData[player.userID].Confirmed) { PrintToChat(player, string.Format(GetMsg("ПрофильДобавленИПодтвержден", player))); return; }
                            if (args[1] == usersdata.VKUsersData[player.userID].ConfirmCode.ToString())
                            {
                                usersdata.VKUsersData[player.userID].Confirmed = true;
                                VKBData.WriteObject(usersdata);
                                PrintToChat(player, string.Format(GetMsg("ПрофильПодтвержден", player)));
                                if (config.GrGifts.VKGroupGifts) { PrintToChat(player, string.Format(GetMsg("ОповещениеОПодарках", player), config.GrGifts.VKGroupUrl)); }
                            }
                            else
                            {
                                PrintToChat(player, string.Format(GetMsg("НеверныйКод", player)));
                            }
                        }
                        else
                        {
                            PrintToChat(player, string.Format(GetMsg("ПрофильНеДобавлен", player)));
                        }
                    }
                    else
                    {
                        if (!usersdata.VKUsersData.ContainsKey(player.userID)) { PrintToChat(player, string.Format(GetMsg("ПрофильНеДобавлен", player))); return; }
                        if (usersdata.VKUsersData[player.userID].Confirmed) { PrintToChat(player, string.Format(GetMsg("ПрофильДобавленИПодтвержден", player))); return; }
                        if (usersdata.VKUsersData.ContainsKey(player.userID))
                        {
                            SendVkMessage(usersdata.VKUsersData[player.userID].VkID, $"Для подтверждения вашего ВК профиля введите в игровой чат команду /vk confirm {usersdata.VKUsersData[player.userID].ConfirmCode}");
                            PrintToChat(player, string.Format(GetMsg("КодОтправлен", player), config.GrGifts.VKGroupUrl));
                        }
                    }
                }
                if (args[0] == "gift")
                {
                    if (config.GrGifts.VKGroupGifts)
                    {
                        if (!usersdata.VKUsersData.ContainsKey(player.userID)) { PrintToChat(player, string.Format(GetMsg("ПрофильНеДобавлен", player))); return; }
                        if (!usersdata.VKUsersData[player.userID].Confirmed) { PrintToChat(player, string.Format(GetMsg("ПрофильНеПодтвержден", player))); return; }
                        if (usersdata.VKUsersData[player.userID].GiftRecived) { PrintToChat(player, string.Format(GetMsg("НаградаУжеПолучена", player))); return; }
                        string url = $"https://api.vk.com/method/groups.isMember?group_id={config.VKAPIT.GroupID}&user_id={usersdata.VKUsersData[player.userID].VkID}&v=5.71&access_token={config.VKAPIT.VKToken}";
                        webrequest.Enqueue(url, null, (code, response) => {
                            var json = JObject.Parse(response);
                            string Result = (string)json["response"];
                            GetGift(code, Result, player);
                        }, this);
                    }
                    else
                    {
                        PrintToChat(player, string.Format(GetMsg("ФункцияОтключена", player)));
                    }
                }
                if (args[0] == "wipealerts")
                {
                    if (!usersdata.VKUsersData.ContainsKey(player.userID)) { PrintToChat(player, string.Format(GetMsg("ПрофильНеДобавлен", player))); return; }
                    if (!usersdata.VKUsersData[player.userID].Confirmed) { PrintToChat(player, string.Format(GetMsg("ПрофильНеПодтвержден", player))); return; }
                    if (usersdata.VKUsersData[player.userID].WipeMsg)
                    {
                        usersdata.VKUsersData[player.userID].WipeMsg = false;
                        VKBData.WriteObject(usersdata);
                        PrintToChat(player, string.Format(GetMsg("ПодпискаОтключена", player)));
                    }
                    else
                    {
                        usersdata.VKUsersData[player.userID].WipeMsg = true;
                        VKBData.WriteObject(usersdata);
                        PrintToChat(player, string.Format(GetMsg("ПодпискаВключена", player)));
                    }
                }
                if (args[0] != "add" && args[0] != "gift" && args[0] != "confirm")
                {
                    PrintToChat(player, string.Format(GetMsg("ДоступныеКоманды", player)));
                    if (config.GrGifts.VKGroupGifts)
                    {
                        PrintToChat(player, string.Format(GetMsg("ОповещениеОПодарках", player), config.GrGifts.VKGroupUrl));
                    }
                }
            }
            else
            {
                PrintToChat(player, string.Format(GetMsg("ДоступныеКоманды", player)));
                if (config.GrGifts.VKGroupGifts)
                {
                    PrintToChat(player, string.Format(GetMsg("ОповещениеОПодарках", player), config.GrGifts.VKGroupUrl));
                }
            }
        }
        private void GetGift(int code, string Result, BasePlayer player)
        {
            if (Result == "1")
            {
                if (config.GrGifts.VKGroupGiftCMD == "none")
                {
                    int FreeSlots = 24 - player.inventory.containerMain.itemList.Count;
                    if (FreeSlots >= config.GrGifts.VKGroupGiftList.Count)
                    {
                        usersdata.VKUsersData[player.userID].GiftRecived = true;
                        VKBData.WriteObject(usersdata);
                        PrintToChat(player, string.Format(GetMsg("НаградаПолучена", player)));
                        if (config.GrGifts.GiftsBool)
                        {
                            Server.Broadcast(string.Format(GetMsg("ПолучилНаграду", player), player.displayName, config.GrGifts.VKGroupUrl));
                        }
                        for (int i = 0; i < config.GrGifts.VKGroupGiftList.Count; i++)
                        {
                            if (Convert.ToInt32(config.GrGifts.VKGroupGiftList.ElementAt(i).Value) > 0)
                            {
                                Item gift = ItemManager.CreateByName(config.GrGifts.VKGroupGiftList.ElementAt(i).Key, Convert.ToInt32(config.GrGifts.VKGroupGiftList.ElementAt(i).Value));
                                gift.MoveToContainer(player.inventory.containerMain, -1, false);
                            }
                        }
                    }
                    else
                    {
                        PrintToChat(player, string.Format(GetMsg("НетМеста", player)));
                    }
                }
                else
                {
                    string cmd = config.GrGifts.VKGroupGiftCMD.Replace("{steamid}", player.userID.ToString());
                    rust.RunServerCommand(cmd);
                    usersdata.VKUsersData[player.userID].GiftRecived = true;
                    VKBData.WriteObject(usersdata);
                    PrintToChat(player, string.Format(GetMsg("НаградаПолученаКоманда", player), config.GrGifts.GiftCMDdesc));
                    if (config.GrGifts.GiftsBool)
                    {
                        Server.Broadcast(string.Format(GetMsg("ПолучилНаграду", player), player.displayName, config.GrGifts.VKGroupUrl));
                    }
                }                
            }
            else
            {
                PrintToChat(player, string.Format(GetMsg("НеВступилВГруппу", player), config.GrGifts.VKGroupUrl));
            }
        }
        private void GiftNotifier()
        {
            if (config.GrGifts.VKGroupGifts)
            {
                foreach (var pl in BasePlayer.activePlayerList)
                {
                    if (!usersdata.VKUsersData.ContainsKey(pl.userID))
                    {
                        PrintToChat(pl, string.Format(GetMsg("ОповещениеОПодарках", pl), config.GrGifts.VKGroupUrl));
                    }
                    else
                    {
                        if (!usersdata.VKUsersData[pl.userID].GiftRecived)
                        {
                            PrintToChat(pl, string.Format(GetMsg("ОповещениеОПодарках", pl), config.GrGifts.VKGroupUrl));
                        }
                    }
                }
            }
        }
        void Update1ServerStatus()
        {
            string status = PrepareStatus(config.StatusStg.StatusText, "status");
            SendVkStatus(status);
        }
        void UpdateMultiServerStatus(string target)
        {
            string text = "";
            string server1 = "";
            string server2 = "";
            string server3 = "";
            string server4 = "";
            string server5 = "";
            if (config.MltServSet.Server1ip != "none")
            {
                var url = "http://" + config.MltServSet.Server1ip + "/status.json";
                webrequest.Enqueue(url, null, (code, response) => {
                    if (response != null || code == 200)
                    {

                        var jsonresponse3 = JsonConvert.DeserializeObject<Dictionary<string, object>>(response, jsonsettings);
                        if (!(jsonresponse3 is Dictionary<string, object>) || jsonresponse3.Count == 0 || !jsonresponse3.ContainsKey("players") || !jsonresponse3.ContainsKey("maxplayers")) return;
                        string online = jsonresponse3["players"].ToString();
                        string slots = jsonresponse3["maxplayers"].ToString();
                        if (config.MltServSet.EmojiStatus) { online = EmojiCounters(online); slots = EmojiCounters(slots); }
                        server1 = "1⃣: " + online.ToString() + "/" + slots.ToString();
                    }
                }, this);
            }
            if (config.MltServSet.Server2ip != "none")
            {
                var url = "http://" + config.MltServSet.Server2ip + "/status.json";
                webrequest.Enqueue(url, null, (code, response) => {
                    if (response != null || code == 200)
                    {

                        var jsonresponse3 = JsonConvert.DeserializeObject<Dictionary<string, object>>(response, jsonsettings);
                        if (!(jsonresponse3 is Dictionary<string, object>) || jsonresponse3.Count == 0 || !jsonresponse3.ContainsKey("players") || !jsonresponse3.ContainsKey("maxplayers")) return;
                        string online = jsonresponse3["players"].ToString();
                        string slots = jsonresponse3["maxplayers"].ToString();
                        if (config.MltServSet.EmojiStatus) { online = EmojiCounters(online); slots = EmojiCounters(slots); }
                        server2 = ", 2⃣: " + online.ToString() + "/" + slots.ToString();
                    }
                }, this);
            }
            if (config.MltServSet.Server3ip != "none")
            {
                var url = "http://" + config.MltServSet.Server3ip + "/status.json";
                webrequest.Enqueue(url, null, (code, response) => {
                    if (response != null || code == 200)
                    {

                        var jsonresponse3 = JsonConvert.DeserializeObject<Dictionary<string, object>>(response, jsonsettings);
                        if (!(jsonresponse3 is Dictionary<string, object>) || jsonresponse3.Count == 0 || !jsonresponse3.ContainsKey("players") || !jsonresponse3.ContainsKey("maxplayers")) return;
                        string online = jsonresponse3["players"].ToString();
                        string slots = jsonresponse3["maxplayers"].ToString();
                        if (config.MltServSet.EmojiStatus) { online = EmojiCounters(online); slots = EmojiCounters(slots); }
                        server3 = ", 3⃣: " + online.ToString() + "/" + slots.ToString();
                    }
                }, this);
            }
            if (config.MltServSet.Server4ip != "none")
            {
                var url = "http://" + config.MltServSet.Server4ip + "/status.json";
                webrequest.Enqueue(url, null, (code, response) => {
                    if (response != null || code == 200)
                    {

                        var jsonresponse3 = JsonConvert.DeserializeObject<Dictionary<string, object>>(response, jsonsettings);
                        if (!(jsonresponse3 is Dictionary<string, object>) || jsonresponse3.Count == 0 || !jsonresponse3.ContainsKey("players") || !jsonresponse3.ContainsKey("maxplayers")) return;
                        string online = jsonresponse3["players"].ToString();
                        string slots = jsonresponse3["maxplayers"].ToString();
                        if (config.MltServSet.EmojiStatus) { online = EmojiCounters(online); slots = EmojiCounters(slots); }
                        server4 = ", 4⃣: " + online.ToString() + "/" + slots.ToString();
                    }
                }, this);
            }
            if (config.MltServSet.Server5ip != "none")
            {
                var url = "http://" + config.MltServSet.Server5ip + "/status.json";
                webrequest.Enqueue(url, null, (code, response) => {
                    if (response != null || code == 200)
                    {

                        var jsonresponse3 = JsonConvert.DeserializeObject<Dictionary<string, object>>(response, jsonsettings);
                        if (!(jsonresponse3 is Dictionary<string, object>) || jsonresponse3.Count == 0 || !jsonresponse3.ContainsKey("players") || !jsonresponse3.ContainsKey("maxplayers")) return;
                        string online = jsonresponse3["players"].ToString();
                        string slots = jsonresponse3["maxplayers"].ToString();
                        if (config.MltServSet.EmojiStatus) { online = EmojiCounters(online); slots = EmojiCounters(slots); }
                        server5 = ", 5⃣: " + online.ToString() + "/" + slots.ToString();
                    }
                }, this);
            }
            Puts("Обработка данных. Статус/обложка будет отправлен(а) через 10 секунд.");
            timer.Once(10f, () =>
            {
                text = server1 + server2 + server3 + server4 + server5;
                if (text != "")
                {
                    if (target == "status")
                    {
                        StatusCheck(text);
                        SendVkStatus(text);
                    }
                    if (target == "label")
                    {
                        text = text.Replace("⃣", "%23");
                        UpdateLabelMultiServer(text);
                    }
                }
                else
                {
                    PrintWarning("Текст для статуса/обложки пуст, не заполнен конфиг или не получены данные с Rust:IO");
                }
            });
        }
        private void MsgAdmin(ConsoleSystem.Arg arg)
        {
            if (arg.IsAdmin != true) { return; }
            if (arg.Args == null)
            {
                PrintWarning($"Текст сообщения отсутсвует, правильная команда |sendmsgadmin сообщение|.");
                return;
            }
            string[] args = arg.Args;
            if (args.Length > 0)
            {
                string text = null;
                if (config.MltServSet.MSSEnable)
                {
                    text = $"[VKBot msgadmin] [Сервер {config.MltServSet.ServerNumber}] " + string.Join(" ", args.Skip(0).ToArray());
                }
                else
                {
                    text = $"[VKBot msgadmin] " + string.Join(" ", args.Skip(0).ToArray());
                }
                SendVkMessage(config.AdmNotify.VkID, text);
                Log("Log", $"|sendmsgadmin| Отправлено новое сообщение администратору: ({text})");
            }
        }
        private void ReportAnswer(ConsoleSystem.Arg arg)
        {
            if (arg.IsAdmin != true) { return; }
            if (arg.Args == null || arg.Args.Count() < 2)
            {
                PrintWarning($"Использование команды - reportanswer 'ID репорта' 'текст ответа'");
                return;
            }
            if (reportsdata.VKReportsData.Count == 0)
            {
                PrintWarning($"База репортов пуста");
                return;
            }
            int reportid = 0;
            reportid = Convert.ToInt32(arg.Args[0]);
            if (reportid == 0 || !reportsdata.VKReportsData.ContainsKey(reportid))
            {
                PrintWarning($"Указан неверный ID репорта");
                return;
            }
            string answer = string.Join(" ", arg.Args.Skip(1).ToArray());            
            if (usersdata.VKUsersData.ContainsKey(reportsdata.VKReportsData[reportid].UserID) && usersdata.VKUsersData[reportsdata.VKReportsData[reportid].UserID].Confirmed)
            {
                string msg = string.Format(GetMsg("ОтветНаРепортВК", 0)) + answer;
                SendVkMessage(usersdata.VKUsersData[reportsdata.VKReportsData[reportid].UserID].VkID, msg);
                PrintWarning($"Ваш ответ был отправлен игроку в ВК.");
                reportsdata.VKReportsData.Remove(reportid);
                ReportsData.WriteObject(reportsdata);
            }
            else
            {
                BasePlayer reciver = BasePlayer.FindByID(reportsdata.VKReportsData[reportid].UserID);
                if (reciver != null)
                {
                    PrintToChat(reciver, string.Format(GetMsg("ОтветНаРепортЧат", reciver)) + answer);
                    PrintWarning($"Ваш ответ был отправлен игроку в игровой чат.");
                    reportsdata.VKReportsData.Remove(reportid);
                    ReportsData.WriteObject(reportsdata);
                }
                else
                {
                    PrintWarning($"Игрок отправивший репорт оффлайн. Невозможно отправить ответ.");
                }
            }
        }
        private void ReportList(ConsoleSystem.Arg arg)
        {
            if (arg.IsAdmin != true) { return; }
            if (reportsdata.VKReportsData.Count == 0)
            {
                PrintWarning($"База репортов пуста");
                return;
            }
            foreach (var report in reportsdata.VKReportsData)
            {
                string status = "offline";
                if (BasePlayer.FindByID(report.Value.UserID) != null) status = "online";
                PrintWarning($"Репорт: ID {report.Key} от игрока {report.Value.Name} ({report.Value.UserID.ToString()}) ({status}). Текст: {report.Value.Text}");
            }
        }
        private void ReportClear(ConsoleSystem.Arg arg)
        {
            if (arg.IsAdmin != true) { return; }
            if (reportsdata.VKReportsData.Count == 0)
            {
                PrintWarning($"База репортов пуста");
                return;
            }
            reportsdata.VKReportsData.Clear();
            ReportsData.WriteObject(reportsdata);
            statdata.Reports = 0;
            StatData.WriteObject(statdata);
            PrintWarning($"База репортов очищена");
        }
        private void GetUserInfo(ConsoleSystem.Arg arg)
        {
            if (arg.IsAdmin != true) { return; }
            if (arg.Args == null)
            {
                PrintWarning($"Введите команду userinfo ник/steamid/vkid для получения информации о игроке из базы vkbot");
                return;
            }
            string[] args = arg.Args;
            if (args.Length > 0)
            {
                bool returned = false;
                int amount = usersdata.VKUsersData.Count;
                if (amount != 0)
                {
                    for (int i = 0; i < amount; i++)
                    {
                        if (usersdata.VKUsersData.ElementAt(i).Value.Name.ToLower().Contains(args[0]) || usersdata.VKUsersData.ElementAt(i).Value.UserID.ToString() == (args[0]) || usersdata.VKUsersData.ElementAt(i).Value.VkID == (args[0]))
                        {
                            returned = true;
                            string text = "Никнейм: " + usersdata.VKUsersData.ElementAt(i).Value.Name + "\nSTEAM: steamcommunity.com/profiles/" + usersdata.VKUsersData.ElementAt(i).Value.UserID + "/";
                            if (usersdata.VKUsersData.ElementAt(i).Value.Confirmed)
                            {
                                text = text + "\nVK: vk.com/id" + usersdata.VKUsersData.ElementAt(i).Value.VkID;
                            }
                            else
                            {
                                text = text + "\nVK: vk.com/id" + usersdata.VKUsersData.ElementAt(i).Value.VkID + " (не подтвержден)";
                            }
                            if (usersdata.VKUsersData.ElementAt(i).Value.Bdate != null && usersdata.VKUsersData.ElementAt(i).Value.Bdate != "noinfo")
                            {
                                text = text + "\nДата рождения: " + usersdata.VKUsersData.ElementAt(i).Value.Bdate;
                            }
                            if (config.TopWPlayersPromo.TopWPlEnabled)
                            {
                                text = text + "\nРазрушено строений: " + usersdata.VKUsersData.ElementAt(i).Value.Raids + "\nУбито игроков: " + usersdata.VKUsersData.ElementAt(i).Value.Kills + "\nНафармил: " + usersdata.VKUsersData.ElementAt(i).Value.Farm;
                            }
                            Puts(text);
                        }
                    }
                }
                if (!returned)
                {
                    Puts("Не найдено игроков с таким именем / steamid / vkid");
                }
            }
        }
        private void SendConfCode(string reciverID, string msg, BasePlayer player)
        {
            string type = "Сообщение";
            string url = "https://api.vk.com/method/messages.send?user_ids=" + reciverID + "&message=" + msg + "&v=5.71&access_token=" + config.VKAPIT.VKToken;
            webrequest.Enqueue(url, null, (code, response) => GetCallbackConfCode(code, response, type, player), this);
        }
        private void CheckPlugins()
        {
            var loadedPlugins = plugins.GetAll().Where(pl => !pl.IsCorePlugin).ToArray();
            var loadedPluginNames = new HashSet<string>(loadedPlugins.Select(pl => pl.Name));
            var unloadedPluginErrors = new Dictionary<string, string>();
            foreach (var loader in Interface.Oxide.GetPluginLoaders())
            {
                foreach (var name in loader.ScanDirectory(Interface.Oxide.PluginDirectory).Except(loadedPluginNames))
                {
                    string msg;
                    unloadedPluginErrors[name] = (loader.PluginErrors.TryGetValue(name, out msg)) ? msg : "Unloaded";
                }
            }
            if (unloadedPluginErrors.Count > 0)
            {
                string text = null;
                if (config.MltServSet.MSSEnable)
                {
                    text = $"[VKBot] [Сервер {config.MltServSet.ServerNumber}] Произошла ошибка загрузки следующих плагинов:";
                }
                else
                {
                    text = $"[VKBot]  Произошла ошибка загрузки следующих плагинов:";
                }
                foreach (var pluginerror in unloadedPluginErrors)
                {
                    text = text + " " + pluginerror.Key + ".";
                }
                if (config.ChNotify.ChNotfEnabled && config.ChNotify.ChNotfSet.Contains("plugins"))
                {
                    SendChatMessage(config.ChNotify.ChatID, text);
                    if (config.ChNotify.AdmMsg) SendVkMessage(config.AdmNotify.VkID, text);
                }
                else
                {
                    SendVkMessage(config.AdmNotify.VkID, text);
                }
            }
        }
        #endregion

        #region VKAPI
        private void SendChatMessage(string chatid, string msg)
        {
            if (msg.Contains("#"))
            {
                msg = msg.Replace("#", "%23");
            }
            string type = "Сообщение в беседу";
            string url = "https://api.vk.com/method/messages.send?chat_id=" + chatid + "&message=" + msg + "&v=5.71&access_token=" + config.ChNotify.ChNotfToken;
            webrequest.Enqueue(url, null, (code, response) => GetCallback(code, response, type), this);
        }
        private void SendVkMessage(string reciverID, string msg)
        {
            if (msg.Contains("#"))
            {
                msg = msg.Replace("#", "%23");
            }
            string type = "Сообщение";
            string url = "https://api.vk.com/method/messages.send?user_ids=" + reciverID + "&message=" + msg + "&v=5.71&access_token=" + config.VKAPIT.VKToken;
            webrequest.Enqueue(url, null, (code, response) => GetCallback(code, response, type), this);
        }
        private void SendVkWall(string msg)
        {
            if (msg.Contains("#"))
            {
                msg = msg.Replace("#", "%23");
            }
            string type = "Пост";
            string url = "https://api.vk.com/method/wall.post?owner_id=-" + config.VKAPIT.GroupID + "&message=" + msg + "&from_group=1&v=5.71&access_token=" + config.VKAPIT.VKTokenApp;
            webrequest.Enqueue(url, null, (code, response) => GetCallback(code, response, type), this);
        }
        private void SendVkStatus(string msg)
        {
            StatusCheck(msg);
            if (msg.Contains("#"))
            {
                msg = msg.Replace("#", "%23");
            }
            string type = "Статус";
            string url = "https://api.vk.com/method/status.set?group_id=" + config.VKAPIT.GroupID + "&text=" + msg + "&v=5.71&access_token=" + config.VKAPIT.VKTokenApp;
            webrequest.Enqueue(url, null, (code, response) => GetCallback(code, response, type), this);
        }
        #endregion

        #region VKBotAPI
        string GetUserVKId(ulong userid)
        {
            if (usersdata.VKUsersData.ContainsKey(userid) && usersdata.VKUsersData[userid].Confirmed)
            {
                var BannedUsers = ServerUsers.BanListString();
                if (!BannedUsers.Contains(userid.ToString()))
                {
                    return usersdata.VKUsersData[userid].VkID;
                }
                else
                {
                    return null;
                }                    
            }
            else
            {
                return null;
            }
        }
        string GetUserLastNotice(ulong userid)
        {
            if (usersdata.VKUsersData.ContainsKey(userid) && usersdata.VKUsersData[userid].Confirmed)
            {
                return usersdata.VKUsersData[userid].LastRaidNotice;
            }
            else
            {
                return null;
            }
        }
        string AdminVkID()
        {
            return config.AdmNotify.VkID;
        }
        private void VKAPISaveLastNotice(ulong userid, string lasttime)
        {
            if (usersdata.VKUsersData.ContainsKey(userid))
            {
                usersdata.VKUsersData[userid].LastRaidNotice = lasttime;
                VKBData.WriteObject(usersdata);
            }
            else
            {
                return;
            }
        }
        private void VKAPIWall(string text, string attachments, bool atimg)
        {
            if (atimg)
            {
                SendVkWall($"{text}&attachments={attachments}");
                Log("vkbotapi", $"Отправлен новый пост на стену: ({text}&attachments={attachments})");
            }
            else
            {
                SendVkWall($"{text}");
                Log("vkbotapi", $"Отправлен новый пост на стену: ({text})");
            }
        }
        private void VKAPIMsg(string text, string attachments, string reciverID, bool atimg)
        {
            if (atimg)
            {
                SendVkMessage(reciverID, $"{text}&attachment={attachments}");
                Log("vkbotapi", $"Отправлено новое сообщение пользователю {reciverID}: ({text}&attachments={attachments})");
            }
            else
            {
                SendVkMessage(reciverID, $"{text}");
                Log("vkbotapi", $"Отправлено новое сообщение пользователю {reciverID}: ({text})");
            }
        }
        private void VKAPIStatus(string msg)
        {
            SendVkStatus(msg);
            Log("vkbotapi", $"Отправлен новый статус: {msg}");
        }
        #endregion

        #region Helpers
        void Log(string filename, string text)
        {
            LogToFile(filename, $"[{DateTime.Now}] {text}", this);
        }
        void GetCallback(int code, string response, string type)
        {
            if (!response.Contains("error"))
            {
                Puts($"{type} отправлен(о): {response}");
            }
            else
            {
                PrintWarning($"{type} не отправлен(о). Файлы лога: /oxide/logs/VKBot/");
                Log("Errors", $"{type} not sended. Error: {response}");
            }
        }
        void GetCallbackConfCode(int code, string response, string type, BasePlayer player)
        {
            if (!response.Contains("error"))
            {
                Puts($"{type} отправлен(о): {response}");
            }
            else
            {
                if (response.Contains("Can't send messages for users without permission"))
                {
                    var RankElements = new CuiElementContainer();
                    var Choose = RankElements.Add(new CuiPanel
                    {
                        Image = { Color = $"0.0 0.0 0 0.35" },
                        RectTransform = { AnchorMin = config.PlChkSet.GUIAnchorMin, AnchorMax = config.PlChkSet.GUIAnchorMax },
                        CursorEnabled = false,
                    }, "Hud", "VKConfGUI");
                    RankElements.Add(new CuiButton
                    {
                        Button = { Color = $"0.34 0.34 0.34 0" },
                        RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" },
                        Text = { Text = string.Format(GetMsg("КодНеОтправлен", player), config.GrGifts.VKGroupUrl), Align = TextAnchor.MiddleCenter, Color = "1 1 1 1", FontSize = 17 }
                    }, Choose);
                    CuiHelper.AddUi(player, RankElements);
                    ConfCods.Add(player);
                    timer.Once(10f, () =>
                    {
                        if (BasePlayer.activePlayerList.Contains(player))
                        {
                            CuiHelper.DestroyUi(player, "VKConfGUI");
                            ConfCods.Remove(player);
                        }
                    });
                }
            }
        }
        private string EmojiCounters(string counter)
        {
            var chars = counter.ToCharArray();
            string emoji = "";
            for (int ctr = 0; ctr < chars.Length; ctr++)
            {
                List<object> digits = new List<object>()
                {
                    "0",
                    "1",
                    "2",
                    "3",
                    "4",
                    "5",
                    "6",
                    "7",
                    "8",
                    "9"
                };
                if (digits.Contains(chars[ctr].ToString()))
                {
                    string replace = chars[ctr] + "⃣";
                    emoji = emoji + replace;
                }
                else
                {
                    emoji = emoji + chars[ctr];
                }
            }
            return emoji;
        }
        private string WipeDate()
        {
            DateTime LastWipe = SaveRestore.SaveCreatedTime.ToLocalTime();
            string LastWipeInfo = LastWipe.ToString("dd.MM");
            return LastWipeInfo;
        }
        private string GetOnline()
        {
            string onlinecounter = "";
            List<ulong> OnlinePlayers = new List<ulong>();
            foreach (var pl in BasePlayer.activePlayerList)
            {
                OnlinePlayers.Add(pl.userID);
            }
            onlinecounter = OnlinePlayers.Count.ToString();
            if (config.StatusStg.OnlWmaxslots)
            {
                var slots = ConVar.Server.maxplayers.ToString();
                onlinecounter = onlinecounter + "/" + slots.ToString();
            }
            return onlinecounter;
        }
        private static List<BasePlayer> FindPlayersOnline(string nameOrIdOrIp)
        {
            var players = new List<BasePlayer>();
            if (string.IsNullOrEmpty(nameOrIdOrIp)) return players;
            foreach (var activePlayer in BasePlayer.activePlayerList)
            {
                if (activePlayer.UserIDString.Equals(nameOrIdOrIp))
                    players.Add(activePlayer);
                else if (!string.IsNullOrEmpty(activePlayer.displayName) && activePlayer.displayName.Contains(nameOrIdOrIp, CompareOptions.IgnoreCase))
                    players.Add(activePlayer);
                else if (activePlayer.net?.connection != null && activePlayer.net.connection.ipaddress.Equals(nameOrIdOrIp))
                    players.Add(activePlayer);
            }
            return players;
        }
        private void StatusCheck(string msg)
        {
            if (msg.Length > 140)
            {
                PrintWarning($"Текст статуса слишком длинный. Измените формат статуса чтобы текст отобразился полностью. Лимит символов в статусе - 140. Длина текста - {msg.Length.ToString()}");
            }
        }
        private bool IsNPC(BasePlayer player)
        {
            if (player is NPCPlayer)
                return true;
            if (!(player.userID >= 76560000000000000L || player.userID <= 0L))
                return true;
            return false;
        }
        private void CheckAdminID()
        {

            if (config.AdmNotify.VkID.Contains("/"))
            {
                string id = config.AdmNotify.VkID.Trim(new char[] { '/' });
                config.AdmNotify.VkID = id;
                Config.WriteObject(config, true);
                PrintWarning("VK ID администратора исправлен. Инструкция по настройке плагина - goo.gl/xRkEUa");
            }
        }
        #endregion

        #region CheatCheking
        private void StartGUI(BasePlayer player)
        {
            var RankElements = new CuiElementContainer();
            var Choose = RankElements.Add(new CuiPanel
            {
                Image = { Color = $"0.0 0.0 0 0.35" },
                RectTransform = { AnchorMin = config.PlChkSet.GUIAnchorMin, AnchorMax = config.PlChkSet.GUIAnchorMax },
                CursorEnabled = false,
            }, "Hud", "AlertGUI");
            RankElements.Add(new CuiButton
            {
                Button = { Color = $"0.34 0.34 0.34 0" },
                RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" },
                Text = { Text = String.Format(config.PlChkSet.PlCheckText), Align = TextAnchor.MiddleCenter, Color = "1 1 1 1", FontSize = config.PlChkSet.PlCheckSize }
            }, Choose);
            CuiHelper.AddUi(player, RankElements);
        }
        private void StopGui(BasePlayer player)
        {
            CuiHelper.DestroyUi(player, "AlertGUI");
        }
        private void StopCheckPlayer(BasePlayer player, string cmd, string[] args)
        {
            if (!permission.UserHasPermission(player.userID.ToString(), config.PlChkSet.PlCheckPerm))
            {
                PrintToChat(player, string.Format(GetMsg("НетПрав", player)));
                return;
            }
            if (args.Length == 1)
            {
                var targets = FindPlayersOnline(args[0]);
                if (targets.Count <= 0)
                {
                    PrintToChat(player, string.Format(GetMsg("ИгрокНеНайден", player)));
                    return;
                }
                if (targets.Count > 1)
                {
                    PrintToChat(player, string.Format(GetMsg("НесколькоИгроков", player)) + string.Join(", ", targets.ConvertAll(p => p.displayName).ToArray()) + ".\nУточните имя игрока.");
                    return;
                }
                var target = targets[0];
                if (target == player)
                {
                    PrintToChat(player, string.Format(GetMsg("ВыНаПроверке", player)));
                    return;
                }
                if (!PlayersCheckList.ContainsValue(target.userID))
                {
                    PrintToChat(player, string.Format(GetMsg("ИгрокНеНаПроверке", player)));
                    return;
                }
                ulong targetid;
                PlayersCheckList.TryGetValue(player.userID, out targetid);
                if (targetid != target.userID)
                {
                    PrintToChat(player, string.Format(GetMsg("ПроверкаДругимМодератором", player)));
                    return;
                }
                StopGui(target);
                PlayersCheckList.Remove(player.userID);
                PrintToChat(player, string.Format(GetMsg("ПроверкаЗакончена", player), target.displayName));
            }
            else
            {
                PrintToChat(player, string.Format(GetMsg("КомандаUnalert", player)));
                return;
            }
        }
        private void StartCheckPlayer(BasePlayer player, string cmd, string[] args)
        {
            if (!permission.UserHasPermission(player.userID.ToString(), config.PlChkSet.PlCheckPerm))
            {
                PrintToChat(player, string.Format(GetMsg("НетПрав", player)));
                return;
            }
            if (!usersdata.VKUsersData.ContainsKey(player.userID) || !usersdata.VKUsersData[player.userID].Confirmed)
            {
                PrintToChat(player, string.Format(GetMsg("ПрофильНеДобавлен", player)));
                return;
            }
            if (args.Length == 1)
            {
                var targets = FindPlayersOnline(args[0]);
                if (targets.Count <= 0)
                {
                    PrintToChat(player, string.Format(GetMsg("ИгрокНеНайден", player)));
                    return;
                }
                if (targets.Count > 1)
                {
                    PrintToChat(player, string.Format(GetMsg("НесколькоИгроков", player)) + string.Join(", ", targets.ConvertAll(p => p.displayName).ToArray()) + ".\nУточните имя игрока.");
                    return;
                }
                var target = targets[0];
                if (target == player)
                {
                    PrintToChat(player, string.Format(GetMsg("ПроверкаСамогоСебя", player)));
                    return;
                }
                if (PlayersCheckList.ContainsValue(target.userID))
                {
                    PrintToChat(player, string.Format(GetMsg("ИгрокУжеНаПроверке", player)));
                    return;
                }
                if (PlayersCheckList.ContainsKey(player.userID))
                {
                    PrintToChat(player, string.Format(GetMsg("ПроверкаНеЗакончена", player)));
                    return;
                }
                StartGUI(target);
                PlayersCheckList.Add(player.userID, target.userID);
                PrintToChat(player, string.Format(GetMsg("ИгрокВызванНаПроверку", player), target.displayName));
            }
            else
            {
                PrintToChat(player, string.Format(GetMsg("КомандаAlert", player)));
                return;
            }
        }
        private void SkypeSending(BasePlayer player, string cmd, string[] args)
        {
            if (!PlayersCheckList.ContainsValue(player.userID))
            {
                PrintToChat(player, string.Format(GetMsg("НеНаПроверке", player)));
                return;
            }
            if (args.Length == 1)
            {
                string reciverid = null;
                for (int i = 0; i < PlayersCheckList.Count; i++)
                {
                    if (PlayersCheckList.ElementAt(i).Value == player.userID)
                    {
                        ulong moderid = PlayersCheckList.ElementAt(i).Key;
                        reciverid = usersdata.VKUsersData[moderid].VkID;
                    }
                }
                if (reciverid != null)
                {
                    if (config.MltServSet.MSSEnable)
                    {
                        msg = "[VKBot] [Сервер " + config.MltServSet.ServerNumber.ToString() + "] " + player.displayName + "(" + player.UserIDString + ") предоставил скайп для проверки: " + args[0] + ". По завершению проверки введите команду /unalert " + player.displayName + " . Ссылка на профиль стим: steamcommunity.com/profiles/" + player.userID.ToString() + "/";
                    }
                    else
                    {
                        msg = "[VKBot] " + player.displayName + "(" + player.UserIDString + ") предоставил скайп для проверки: " + args[0] + ". Ссылка на профиль стим: steamcommunity.com/profiles/" + player.userID.ToString() + "/";
                    }
                    if (usersdata.VKUsersData.ContainsKey(player.userID) && usersdata.VKUsersData[player.userID].Confirmed)
                    {
                        msg = msg + " . Ссылка на профиль ВК: vk.com/id" + usersdata.VKUsersData[player.userID].VkID;
                    }
                    SendVkMessage(reciverid, msg);
                    PrintToChat(player, string.Format(GetMsg("СкайпОтправлен", player)));
                }
            }
            else
            {
                PrintToChat(player, string.Format(GetMsg("КомандаСкайп", player)));
                return;
            }
        }
        #endregion

        #region TopWipePlayersStatsAndPromo
        private string BannedUsers = ServerUsers.BanListString();
        private ulong GetTopRaider()
        {            
            int max = 0;
            ulong TopID = 0;
            int amount = usersdata.VKUsersData.Count;
            if (amount != 0)
            {
                for (int i = 0; i < amount; i++)
                {
                    if (usersdata.VKUsersData.ElementAt(i).Value.Raids > max && !BannedUsers.Contains(usersdata.VKUsersData.ElementAt(i).Value.UserID.ToString()))
                    {
                        max = usersdata.VKUsersData.ElementAt(i).Value.Raids;
                        TopID = usersdata.VKUsersData.ElementAt(i).Value.UserID;
                    }
                }
            }
            if (max != 0)
            {
                return TopID;
            }
            else
            {
                return 0;
            }
        }
        private ulong GetTopKiller()
        {
            int max = 0;
            ulong TopID = 0;
            int amount = usersdata.VKUsersData.Count;
            if (amount != 0)
            {
                for (int i = 0; i < amount; i++)
                {
                    if (usersdata.VKUsersData.ElementAt(i).Value.Kills > max && !BannedUsers.Contains(usersdata.VKUsersData.ElementAt(i).Value.UserID.ToString()))
                    {
                        max = usersdata.VKUsersData.ElementAt(i).Value.Kills;
                        TopID = usersdata.VKUsersData.ElementAt(i).Value.UserID;
                    }
                }
            }
            if (max != 0)
            {
                return TopID;
            }
            else
            {
                return 0;
            }
        }
        private ulong GetTopFarmer()
        {
            int max = 0;
            ulong TopID = 0;
            int amount = usersdata.VKUsersData.Count;
            if (amount != 0)
            {
                for (int i = 0; i < amount; i++)
                {
                    if (usersdata.VKUsersData.ElementAt(i).Value.Farm > max && !BannedUsers.Contains(usersdata.VKUsersData.ElementAt(i).Value.UserID.ToString()))
                    {
                        max = usersdata.VKUsersData.ElementAt(i).Value.Farm;
                        TopID = usersdata.VKUsersData.ElementAt(i).Value.UserID;
                    }
                }
            }
            if (max != 0)
            {
                return TopID;
            }
            else
            {
                return 0;
            }
        }
        private void SendPromoMsgsAndPost()
        {
            var traider = GetTopRaider();
            var tkiller = GetTopKiller();
            var tfarmer = GetTopFarmer();
            if (config.TopWPlayersPromo.TopPlPost)
            {
                bool check = false;
                string text = "Топ игроки прошедшего вайпа:";
                if (traider != 0)
                {
                    text = text + "\nТоп рэйдер: " + usersdata.VKUsersData[traider].Name;
                    check = true;
                }
                if (tkiller != 0)
                {
                    text = text + "\nТоп киллер: " + usersdata.VKUsersData[tkiller].Name;
                    check = true;
                }
                if (tfarmer != 0)
                {
                    text = text + "\nТоп фармер: " + usersdata.VKUsersData[tfarmer].Name;
                    check = true;
                }
                if (config.TopWPlayersPromo.TopPlPromoGift)
                {
                    text = text + "\nТоп игроки получают в качестве награды промокод на баланс в магазине.";
                }
                if (check)
                {
                    if (config.TopWPlayersPromo.TopPlPostAtt != "none")
                    {
                        text = text + "&attachments=" + config.TopWPlayersPromo.TopPlPostAtt;
                    }
                    SendVkWall(text);
                }
            }
            if (traider != 0 && config.TopWPlayersPromo.TopPlPromoGift && usersdata.VKUsersData.ContainsKey(traider) && usersdata.VKUsersData[traider].Confirmed)
            {
                string text = string.Format(GetMsg("СообщениеИгрокуТопПромо", 0), "рэйдер", config.TopWPlayersPromo.TopRaiderPromo, config.TopWPlayersPromo.StoreUrl);
                if (config.TopWPlayersPromo.TopRaiderPromoAtt != "none") text = text + "&attachments=" + config.TopWPlayersPromo.TopRaiderPromoAtt;
                string reciver = usersdata.VKUsersData[traider].VkID;
                SendVkMessage(reciver, text);
            }
            if (tkiller != 0 && config.TopWPlayersPromo.TopPlPromoGift && usersdata.VKUsersData.ContainsKey(tkiller) && usersdata.VKUsersData[tkiller].Confirmed)
            {
                string text = string.Format(GetMsg("СообщениеИгрокуТопПромо", 0), "киллер", config.TopWPlayersPromo.TopKillerPromo, config.TopWPlayersPromo.StoreUrl);
                if (config.TopWPlayersPromo.TopKillerPromoAtt != "none") text = text + "&attachments=" + config.TopWPlayersPromo.TopKillerPromoAtt;
                string reciver = usersdata.VKUsersData[tkiller].VkID;
                SendVkMessage(reciver, text);
            }
            if (tfarmer != 0 && config.TopWPlayersPromo.TopPlPromoGift && usersdata.VKUsersData.ContainsKey(tfarmer) && usersdata.VKUsersData[tfarmer].Confirmed)
            {
                string text = string.Format(GetMsg("СообщениеИгрокуТопПромо", 0), "фармер", config.TopWPlayersPromo.TopFarmerPromo, config.TopWPlayersPromo.StoreUrl);
                if (config.TopWPlayersPromo.TopFarmerPromoAtt != "none") text = text + "&attachments=" + config.TopWPlayersPromo.TopFarmerPromoAtt;
                string reciver = usersdata.VKUsersData[tfarmer].VkID;
                SendVkMessage(reciver, text);
            }
        }
        private string PromoGenerator()
        {
            List<string> Chars = new List<string>() { "A", "1", "B", "2", "C", "3", "D", "4", "F", "5", "G", "6", "H", "7", "I", "8", "J", "9", "K", "0", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
            string promo = "";
            for (int i = 0; i < 6; i++)
            {
                promo = promo + Chars.GetRandom();
            }
            return promo;
        }
        private void SetRandomPromo()
        {
            config.TopWPlayersPromo.TopFarmerPromo = PromoGenerator();
            config.TopWPlayersPromo.TopKillerPromo = PromoGenerator();
            config.TopWPlayersPromo.TopRaiderPromo = PromoGenerator();
            Config.WriteObject(config, true);
            string msg = "[VKBot]";
            if (config.MltServSet.MSSEnable)
            {
                msg = msg + " [Сервер " + config.MltServSet.ServerNumber.ToString() + "]";
            }
            msg = msg + " В настройки добавлены новые промокоды: \nТоп рэйдер - " + config.TopWPlayersPromo.TopRaiderPromo + "\nТоп киллер - " + config.TopWPlayersPromo.TopKillerPromo + "\nТоп фармер - " + config.TopWPlayersPromo.TopFarmerPromo;
            SendVkMessage(config.AdmNotify.VkID, msg);
        }
        #endregion

        #region DynamicLabelVK
        private void UpdateVKLabel()
        {
            string url = config.DGLSet.DLUrl + "?";
            int count = 0;
            if (config.DGLSet.DLText1 != "none")
            {
                if (count == 0)
                {
                    url = url + "t1=" + PrepareStatus(config.DGLSet.DLText1, "label");
                    count++;
                }
                else
                {
                    url = url + "&t1=" + PrepareStatus(config.DGLSet.DLText1, "label");
                }
            }
            if (config.DGLSet.DLText2 != "none")
            {
                if (count == 0)
                {
                    url = url + "t2=" + PrepareStatus(config.DGLSet.DLText2, "label");
                    count++;
                }
                else
                {
                    url = url + "&t2=" + PrepareStatus(config.DGLSet.DLText2, "label");
                }
            }
            if (config.DGLSet.DLText3 != "none")
            {
                if (count == 0)
                {
                    url = url + "t3=" + PrepareStatus(config.DGLSet.DLText3, "label");
                    count++;
                }
                else
                {
                    url = url + "&t3=" + PrepareStatus(config.DGLSet.DLText3, "label");
                }
            }
            if (config.DGLSet.DLText4 != "none")
            {
                if (count == 0)
                {
                    url = url + "t4=" + PrepareStatus(config.DGLSet.DLText4, "label");
                    count++;
                }
                else
                {
                    url = url + "&t4=" + PrepareStatus(config.DGLSet.DLText4, "label");
                }
            }
            if (config.DGLSet.DLText5 != "none")
            {
                if (count == 0)
                {
                    url = url + "t5=" + PrepareStatus(config.DGLSet.DLText5, "label");
                    count++;
                }
                else
                {
                    url = url + "&t5=" + PrepareStatus(config.DGLSet.DLText5, "label");
                }
            }
            if (config.DGLSet.DLText6 != "none")
            {
                if (count == 0)
                {
                    url = url + "t6=" + PrepareStatus(config.DGLSet.DLText6, "label");
                    count++;
                }
                else
                {
                    url = url + "&t6=" + PrepareStatus(config.DGLSet.DLText6, "label");
                }
            }
            if (config.DGLSet.DLText7 != "none")
            {
                if (count == 0)
                {
                    url = url + "t7=" + PrepareStatus(config.DGLSet.DLText7, "label");
                    count++;
                }
                else
                {
                    url = url + "&t7=" + PrepareStatus(config.DGLSet.DLText7, "label");
                }
            }
            webrequest.Enqueue(url, null, (code, response) => DLResult(code, response), this);
        }
        private void DLResult(int code, string response)
        {
            if (response.Contains("good"))
            {
                Puts("Обложка группы обновлена");
            }
            else
            {
                Puts("Прозошла ошибка обновления обложки, проверьте настройки.");
            }
        }
        private void ULabel(ConsoleSystem.Arg arg)
        {
            if (arg.IsAdmin != true) { return; }
            if (config.DGLSet.DLEnable && config.DGLSet.DLUrl != "none")
            {
                if (config.DGLSet.DLMSEnable)
                {
                    UpdateMultiServerStatus("label");
                }
                else
                {
                    UpdateVKLabel();
                }                
            }
            else
            {
                PrintWarning($"Функция обновления обложки отключена, или не указана ссылка на скрипт обновления.");
            }
        }
        private void UpdateLabelMultiServer(string text)
        {
            string url = config.DGLSet.DLUrl + "?t1=" + text; //подставить переменную для выбора места?
            webrequest.Enqueue(url, null, (code, response) => DLResult(code, response), this);
        }
        #endregion

        #region Langs
        private void LoadMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                {"ПоздравлениеИгрока", "<size=17><color=#049906>Администрация сервера поздравляет вас с днем рождения! В качестве подарка мы добавили вас в группу с рейтами x4 и китом bday!</color></size>"},
                {"ДеньРожденияИгрока", "<size=17>Администрация сервера поздравляет игрока <color=#049906>{0}</color> с Днем Рождения!</size>"},
                {"ИгрокОтключился", "<size=17>Игрок вызванный на проверку покинул сервер. Причина: <color=#049906>{0}</color>.</size>"},
                {"МодераторОтключился", "<size=17>Модератор отключился от сервера, ожидайте следующей проверки.</size>"},
                {"РепортОтправлен", "<size=17>Ваше сообщение было отправлено администратору.\n<color=#049906>ВНИМАНИЕ!</color>\n{0}</size>"},
                {"КомандаРепорт", "<size=17>Введите команду <color=#049906>/report сообщение</color>.\n<color=#049906>ВНИМАНИЕ!</color>\n{0}</size>"},
                {"ФункцияОтключена", "<size=17><color=#049906>Данная функция отключена администратором.</color>.</size>"},
                {"ПрофильДобавленИПодтвержден", "<size=17>Вы уже добавили и подтвердили свой профиль.</size>"},
                {"ПрофильДобавлен", "<size=17>Вы уже добавили свой профиль. Если вам не пришел код подтверждения, введите команду <color=#049906>/vk confirm</color></size>"},
                {"ДоступныеКоманды", "<size=17>Список доступных команд:\n /vk add ссылка на вашу страницу - добавление вашего профиля ВК в базу.\n /vk confirm - подтверждение вашего профиля ВК</size>"},
                {"НеправильнаяСсылка", "<size=17>Ссылка на страницу должна быть вида |vk.com/testpage| или |vk.com/id0000|</size>"},
                {"ПрофильПодтвержден", "<size=17>Вы подтвердили свой профиль! Спасибо!</size>"},
                {"ОповещениеОПодарках", "<size=17>Вы можете получить награду, если вступили в нашу группу <color=#049906>{0}</color> введя команду <color=#049906>/vk gift.</color></size>"},
                {"НеверныйКод", "<size=17>Неверный код подтверждения.</size>"},
                {"ПрофильНеДобавлен", "<size=17>Сначала добавьте и подтвердите свой профиль командой <color=#049906>/vk add ссылка на вашу страницу.</color> Ссылка на должна быть вида |vk.com/testpage| или |vk.com/id0000|</size>"},
                {"КодОтправлен", "<size=17>Вам был отправлен код подтверждения. Если сообщение не пришло, зайдите в группу <color=#049906>{0}</color> и напишите любое сообщение. После этого введите команду <color=#049906>/vk confirm</color></size>"},
                {"ПрофильНеПодтвержден", "<size=17>Сначала подтвердите свой профиль ВК командой <color=#049906>/vk confirm</color></size>"},
                {"НаградаУжеПолучена", "<size=17>Вы уже получили свою награду.</size>"},
                {"ПодпискаОтключена", "<size=17>Вы <color=#049906>отключили</color> подписку на сообщения о вайпах сервера. Что бы включить подписку снова, введите команду <color=#049906>/vk wipealerts</color></size>"},
                {"ПодпискаВключена", "<size=17>Вы <color=#049906>включили</color> подписку на сообщения о вайпах сервера. Что бы отключить подписку, введите команду <color=#049906>/vk wipealerts</color></size>"},
                {"НаградаПолучена", "<size=17>Вы получили свою награду! Проверьте инвентарь!</size>"},
                {"ПолучилНаграду", "<size=17>Игрок <color=#049906>{0}</color> получил награду за вступление в группу <color=#049906>{1}.</color>\nХочешь тоже получить награду? Введи в чат команду <color=#049906>/vk gift</color>.</size>"},
                {"НетМеста", "<size=17>Недостаточно места для получения награды.</size>"},
                {"НаградаПолученаКоманда", "<size=17>За вступление в группу нашего сервера вы получили {0}</size>"},
                {"НеВступилВГруппу", "<size=17>Вы не являетесь участником группы <color=#049906>{0}</color></size>"},
                {"ОтветНаРепортЧат", "<size=17><color=#049906>Администратор ответил на ваше сообщение:</color>\n</size>"},
                {"ОтветНаРепортВК", "<size=17><color=#049906>Администратор ответил на ваше сообщение:</color>\n</size>"},
                {"КодНеОтправлен", "Мы не можем отправить вам код подтверждения.\nЗайдите в группу <color=#049906>{0}</color> и напишите любое сообщение\nПосле этого введите в чат команду <color=#049906>/vk confirm</color>"},
                {"НетПрав", "<size=17>У вас нет прав для использования данной команды</size>"},
                {"ИгрокНеНайден", "<size=17>Игрок не найден</size>"},
                {"НесколькоИгроков", "<color=#049906>Найдено несколько игроков:\n</color>"},
                {"ВыНаПроверке", "<size=17>Вас вызвали на проверку, вы не можете сами ее закончить</size>"},
                {"ИгрокНеНаПроверке", "<size=17>Этого игрока не вызывали на проверку</size>"},
                {"ПроверкаДругимМодератором", "<size=17>Вы не можете закончить проверку, начатую другим модератором</size>"},
                {"ПроверкаЗакончена", "<size=17>Вы закончили проверку игрока <color=#049906>{0}</color></size>"},
                {"КомандаUnalert", "<size=17>Команда <color=#049906>/unalert имя_игрока</color> или <color=#049906>/unalert steamid</color></size>"},
                {"ПроверкаСамогоСебя", "<size=17>Вы не можете проверить сами себя.</size>"},
                {"ИгрокУжеНаПроверке", "<size=17>Этого игрока уже вызвали на проверку</size>"},
                {"ПроверкаНеЗакончена", "<size=17>Сначала закончите предыдущую проверку</size>"},
                {"ИгрокВызванНаПроверку", "<size=17>Вы вызвали игрока <color=#049906>{0}</color> на проверку</size>"},
                {"НеНаПроверке", "<size=17>Вас не вызывали на проверку. Ваш скайп нам не нужен <color=#049906>:)</color></size>"},
                {"СкайпОтправлен", "<size=17>Ваш скайп был отправлен модератору. Ожидайте звонка.</size>"},
                {"КомандаСкайп", "<size=17>Команда <color=#049906>/skype НИК в СКАЙПЕ</color></size>"},
                {"КомандаAlert", "<size=17>Команда <color=#049906>/alert имя_игрока</color> или <color=#049906>/alert steamid</color></size>"},
                {"СообщениеИгрокуТопПромо", "Поздравляем! Вы Топ {0} по результатам этого вайпа! В качестве награды, вы получаете промокод {1} на баланс в нашем магазине! {2}"}
            }, this);
        }
        string GetMsg(string key, BasePlayer player = null) => GetMsg(key, player.UserIDString);
        string GetMsg(string key, object userID = null) => lang.GetMessage(key, this, userID == null ? null : userID.ToString());
        #endregion
    }
}
