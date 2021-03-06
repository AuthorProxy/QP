﻿using System.Collections.Generic;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;

namespace Quantumart.QP8.BLL
{
    public class BackendAction : BizObject
    {
        /// <summary>
        /// Идентификатор действия
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор типа действия
        /// </summary>
        public int TypeId { get; set; }

        /// <summary>
        /// Идентификатор типа сущности
        /// </summary>
        public int EntityTypeId { get; set; }

        /// <summary>
        /// название действия
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Непереведенное название действия
        /// </summary>
        public string NotTranslatedName { get; set; }

        /// <summary>
        /// Краткое название действия
        /// </summary>
        public string ShortName { get; set; }

        /// <summary>
        /// Код действия
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// URL действия контроллера
        /// </summary>
        public string ControllerActionUrl { get; set; }

        /// <summary>
        /// Дополнительный URL действия контроллера
        /// </summary>
        public string AdditionalControllerActionUrl { get; set; }

        /// <summary>
        /// Текст предупреждения, которое выводится перед запуском действия
        /// </summary>
        [LocalizedDisplayName("ConfirmPhrase", NameResourceType = typeof(CustomActionStrings))]
        [MaxLengthValidator(1000, MessageTemplateResourceName = "ConfirmPhraseLengthExceeded", MessageTemplateResourceType = typeof(EntityObjectStrings))]
        public string ConfirmPhrase { get; set; }

        /// <summary>
        /// Признак того, что для выполнения действия требуется пользовательский интерфейс
        /// </summary>
        [LocalizedDisplayName("IsInterface", NameResourceType = typeof(CustomActionStrings))]
        public bool IsInterface { get; set; }

        /// <summary>
        /// Имеется преэкшн
        /// </summary>
        [LocalizedDisplayName("HasPreAction", NameResourceType = typeof(CustomActionStrings))]
        public bool HasPreAction { get; set; }

        /// <summary>
        /// Имеются настройки
        /// </summary>
        [LocalizedDisplayName("HasSettings", NameResourceType = typeof(CustomActionStrings))]
        public bool HasSettings { get; set; }

        /// <summary>
        /// Признак того, что для выполнения действия требуется всплывающее окно
        /// </summary>
        [LocalizedDisplayName("IsWindow", NameResourceType = typeof(CustomActionStrings))]
        public bool IsWindow { get; set; }

        /// <summary>
        /// Ширина окна
        /// </summary>
        [LocalizedDisplayName("WindowWidth", NameResourceType = typeof(CustomActionStrings))]
        public int? WindowWidth { get; set; }

        /// <summary>
        /// Высота окна
        /// </summary>
        [LocalizedDisplayName("WindowHeight", NameResourceType = typeof(CustomActionStrings))]
        public int? WindowHeight { get; set; }

        /// <summary>
        /// Представление, выбранное по умолчанию
        /// </summary>
        public ViewType DefaultViewType { get; set; }

        /// <summary>
        /// Разрешает поиск
        /// </summary>
        public bool AllowSearch { get; set; }

        /// <summary>
        /// Разрешает предварительный просмотр
        /// </summary>
        public bool AllowPreview { get; set; }

        /// <summary>
        /// Идентификатор родительского действия
        /// </summary>
        public int? ParentId { get; set; }

        /// <summary>
        /// Тип действия
        /// </summary>
        public BackendActionType ActionType { get; set; }

        /// <summary>
        /// Тип сущности
        /// </summary>
        public EntityType EntityType { get; set; }

        /// <summary>
        /// Список представлений
        /// </summary>
        public List<BackendActionView> Views { get; set; }

        /// <summary>
        /// Идентификатор действия, которое должно быть произведено после успешного выполнения текущего действия
        /// </summary>
        public int? NextSuccessfulActionId { get; set; }

        /// <summary>
        /// Код действия, которое должно быть произведено после успешного выполнения текущего действия
        /// </summary>
        public string NextSuccessfulActionCode { get; set; }

        /// <summary>
        /// Идентификатор действия, которое должно быть произведено после неуспешного выполнения текущего действия
        /// </summary>
        public int? NextFailedActionId { get; set; }

        /// <summary>
        /// Код действия, которое должно быть произведено после неуспешного выполнения текущего действия
        /// </summary>
        public string NextFailedActionCode { get; set; }

        /// <summary>
        /// Признак того, что действие является пользовательским
        /// </summary>
        public bool IsCustom { get; set; }

        /// <summary>
        /// Признак того, что операция длительная и многошаговая
        /// </summary>
        public bool IsMultistep { get; set; }

        /// <summary>
        /// Кнопки тулбара
        /// </summary>
        public IEnumerable<ToolbarButton> ToolbarButtons { get; set; }

        /// <summary>
        /// Элементы контекстного меню
        /// </summary>
        public IEnumerable<ContextMenuItem> ContextMenuItems { get; set; }

        /// <summary>
        /// Id вкладки
        /// </summary>
        public int? TabId { get; set; }

        /// <summary>
        /// Исключить коды
        /// </summary>
        public string[] ExcludeCodes { get; set; }

        /// <summary>
        /// Лимит объектов при превышении которого действие обрабатывается как многошаговое
        /// </summary>
        public int? EntityLimit { get; set; }
    }
}
