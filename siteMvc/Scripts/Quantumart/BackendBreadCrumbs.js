window.EVENT_TYPE_BREAD_CRUMBS_ITEM_CLICK = 'OnBreadCrumbsItemClick';
window.EVENT_TYPE_BREAD_CRUMBS_ITEM_CTRL_CLICK = 'OnBreadCrumbsItemCtrlClick';
window.EVENT_TYPE_BREAD_CRUMBS_ITEM_CONTEXT_CLICK = 'OnBreadCrumbsContextMenuItemClick';

Quantumart.QP8.BackendBreadCrumbs = function (breadCrumbsElementId, options) {
  Quantumart.QP8.BackendBreadCrumbs.initializeBase(this);
  this._breadCrumbsElementId = breadCrumbsElementId;
  if ($q.isObject(options)) {
    if (options.breadCrumbsContainerElementId) {
      this._breadCrumbsContainerElementId = options.breadCrumbsContainerElementId;
    }

    if (options.documentHost) {
      this._documentHost = options.documentHost;
    }

    if (options.manager) {
      this._manager = options.manager;
    }

    if (options.contextMenuManager) {
      this._contextMenuManagerComponent = options.contextMenuManager;
    }
  }

  $q.bindProxies.call(this, [
    '_onBreadCrumbsItemClick',
    '_onGetBreadCrumbsListSuccess',
    '_onGetBreadCrumbsListError',

    '_onContextMenu',
    '_onContextMenuShowing',
    '_onContextMenuItemClicking',
    '_onContextMenuHidden'
  ]);
};

Quantumart.QP8.BackendBreadCrumbs.prototype = {
  _breadCrumbsElementId: '',
  _breadCrumbsElement: null,
  _breadCrumbsItemListElement: null,
  _breadCrumbsContainerElementId: '',
  _documentHost: null,
  _stopDeferredOperations: false,
  _addItemsCallback: null,
  _manager: null,

  ITEM_CLASS_NAME: 'item',
  ITEM_BUSY_CLASS_NAME: 'busy',

  ITEM_SELECTORS: 'li.item',
  ITEM_WITH_LINK_SELECTORS: 'li.item:has(a)',
  ITEM_LAST_WITH_LINK_SELECTORS: 'li.item:has(a):last',

  _onBreadCrumbsItemClickHandler: null,
  _onGetBreadCrumbsListSuccessHandler: null,
  _onGetBreadCrumbsListErrorHandler: null,

  _contextMenuManagerComponent: null,
  _onContextMenuHandler: null,
  _onContextMenuShowingHandler: null,
  _onContextMenuItemClickingHandler: null,
  _onContextMenuHiddenHandler: null,

  get_contextMenuManager () {
    return this._contextMenuManager;
  },
  set_contextMenuManager (value) {
    this._contextMenuManager = value;
  },
  get_manager () {
    return this._manager;
  },
  set_manager (value) {
    this._manager = value;
  },
  get_breadCrumbsElementId () {
    return this._breadCrumbsElementId;
  },
  set_breadCrumbsElementId (value) {
    this._breadCrumbsElementId = value;
  },
  get_breadCrumbsElement () {
    return this._breadCrumbsElement;
  },
  get_breadCrumbsContainerElementId () {
    return this._breadCrumbsContainerElementId;
  },
  set_breadCrumbsContainerElementId (value) {
    this._breadCrumbsContainerElementId = value;
  },

  initialize () {
    let $breadCrumbs = $(`#${this._breadCrumbsElementId}`);
    let $breadCrumbsItemList = $breadCrumbs.find('ul:first');
    if (!$breadCrumbs.length) {
      $breadCrumbs = $('<div />', { id: this._breadCrumbsElementId, class: 'breadCrumbs', css: { display: 'none' } });
      $breadCrumbsItemList = $('<ul />');
      $breadCrumbs.append($breadCrumbsItemList);
      if (!$q.isNullOrWhiteSpace(this._breadCrumbsContainerElementId)) {
        $(`#${this._breadCrumbsContainerElementId}`).append($breadCrumbs);
      } else {
        $('body:first').append($breadCrumbs);
      }

      this._breadCrumbsElement = $breadCrumbs.get(0);
      this._breadCrumbsItemListElement = $breadCrumbsItemList.get(0);
      this._attachBreadCrumbsEventHandlers();
    } else {
      this._breadCrumbsElement = $breadCrumbs.get(0);
      this._breadCrumbsItemListElement = $breadCrumbsItemList.get(0);
    }
  },

  _attachBreadCrumbsEventHandlers () {
    if (this._contextMenuManagerComponent != null) {
      this._contextMenuManagerComponent.attachObserver(window.EVENT_TYPE_CONTEXT_MENU_SHOWING, this._onContextMenuShowingHandler);
      this._contextMenuManagerComponent.attachObserver(window.EVENT_TYPE_CONTEXT_MENU_ITEM_CLICKING, this._onContextMenuItemClickingHandler);
      this._contextMenuManagerComponent.attachObserver(window.EVENT_TYPE_CONTEXT_MENU_HIDDEN, this._onContextMenuHiddenHandler);
    }

    $(this._breadCrumbsItemListElement)
      .on('click', this.ITEM_WITH_LINK_SELECTORS, this._onBreadCrumbsItemClickHandler)
      .on('mouseup', this.ITEM_WITH_LINK_SELECTORS, this._onBreadCrumbsItemClickHandler)
      .on(this._contextMenuManagerComponent.getContextMenuEventType(), this.ITEM_SELECTORS, this._onContextMenuHandler);
  },

  _detachBreadCrumbsHandlers () {
    if (this._contextMenuManagerComponent != null) {
      this._contextMenuManagerComponent.detachObserver(window.EVENT_TYPE_CONTEXT_MENU_SHOWING, this._onContextMenuShowingHandler);
      this._contextMenuManagerComponent.detachObserver(window.EVENT_TYPE_CONTEXT_MENU_ITEM_CLICKING, this._onContextMenuItemClickingHandler);
      this._contextMenuManagerComponent.detachObserver(window.EVENT_TYPE_CONTEXT_MENU_HIDDEN, this._onContextMenuHiddenHandler);
    }

    $(this._breadCrumbsItemListElement)
      .off('click', this.ITEM_WITH_LINK_SELECTORS, this._onBreadCrumbsItemClickHandler)
      .off('mouseup', this.ITEM_WITH_LINK_SELECTORS, this._onBreadCrumbsItemClickHandler)
      .off(this._contextMenuManagerComponent.getContextMenuEventType(), this.ITEM_SELECTORS, this._onContextMenuHandler);
  },

  getItems () {
    return $(this._breadCrumbsItemListElement).find(this.ITEM_SELECTORS);
  },

  getLastItemButOne () {
    return $(this._breadCrumbsItemListElement).find(this.ITEM_LAST_WITH_LINK_SELECTORS);
  },

  getItem (item) {
    let $item = null;
    if ($q.isObject(item)) {
      return $q.toJQuery(item);
    } else if ($q.isString(item)) {
      $item = $(`li[code='${item}']`, this._breadCrumbsItemListElement);
      if ($item.length === 0) {
        $item = null;
      }

      return $item;
    }
  },

  getItemValue (itemElem) {
    const $item = this.getItem(itemElem);
    let itemValue = '';

    if (!$q.isNullOrEmpty($item)) {
      itemValue = $item.attr('code');
    } else {
      $q.alertError('Ошибка. Отсутствует значение.');
      return;
    }

    return itemValue;
  },

  getItemText (item) {
    const $item = this.getItem(item);
    let itemText = '';

    if (!$q.isNullOrEmpty($item)) {
      itemText = $('span.text', $item).text();
    }

    return itemText;
  },

  showBreadCrumbs () {
    $(this._breadCrumbsElement).show();
  },

  hideBreadCrumbs () {
    $(this._breadCrumbsElement).hide();
  },

  markBreadCrumbsAsBusy () {
    $(this._breadCrumbsItemListElement).addClass(this.ITEM_BUSY_CLASS_NAME);
  },

  unmarkBreadCrumbsAsBusy () {
    $(this._breadCrumbsItemListElement).removeClass(this.ITEM_BUSY_CLASS_NAME);
  },

  _isBusy () {
    $(this._breadCrumbsItemListElement).hasClass(this.ITEM_BUSY_CLASS_NAME);
  },

  addItemsToBreadCrumbs (callback) {
    const host = this._documentHost;
    this._addItemsCallback = callback;
    Quantumart.QP8.BackendBreadCrumbs.getBreadCrumbsList(
      host.get_entityTypeCode(),
      host.get_entityId(),
      host.get_parentEntityId(),
      host.get_actionCode(),
      this._onGetBreadCrumbsListSuccessHandler,
      this._onGetBreadCrumbsListErrorHandler
    );
  },

  _onGetBreadCrumbsListError (jqXHR, textStatus, errorThrown) {
    if (this._stopDeferredOperations) {
      return;
    }

    $q.processGenericAjaxError(jqXHR);
    $q.callFunction(this._addItemsCallback);
  },

  _onGetBreadCrumbsListSuccess (data, textStatus, jqXHR) {
    if (this._stopDeferredOperations) {
      return;
    }

    const items = data;
    if (!$q.isNull(items)) {
      const itemCount = items.length;
      if (itemCount > 0) {
        const $breadCrumbsItemList = $(this._breadCrumbsItemListElement);
        const itemsHtml = new $.telerik.stringBuilder();
        for (let itemIndex = itemCount - 1; itemIndex >= 0; itemIndex--) {
          const item = items[itemIndex];
          this._getItemHtml(itemsHtml, item);
        }

        $breadCrumbsItemList.html(itemsHtml.string());
        this._extendItemElements(items);
        $q.callFunction(this._addItemsCallback);
      } else {
        this.removeItemsFromBreadCrumbs(this._addItemsCallback);
      }
    }
  },

  removeItemsFromBreadCrumbs (callback) {
    const $breadCrumbsItemList = $(this._breadCrumbsItemListElement);
    $breadCrumbsItemList.empty();
    $q.callFunction(callback);
  },

  _getItemHtml (html, dataItem, isSelectedItem) {
    const itemCode = Quantumart.QP8.BackendBreadCrumbsManager.getInstance().generateItemCode(dataItem.Code, dataItem.ParentId, dataItem.Id);
    html.cat(`<li code="${$q.htmlEncode(itemCode)}" class="${this.ITEM_CLASS_NAME}"></li>\n`);
  },

  _extendItemElement (itemElem, dataItem, isSelectedItem) {
    const $item = this.getItem(itemElem);
    if (!$q.isNullOrEmpty($item)) {
      const html = new $.telerik.stringBuilder();
      html
        .catIf('  <a href="javascript:void(0);">', !isSelectedItem)
        .cat('<span class="text"')
        .cat(" title='(")
        .cat(dataItem.Id)
        .cat(') ')
        .cat(dataItem.EntityTypeName)
        .cat(' \"')
        .cat($q.htmlEncode(dataItem.Title))
        .cat('\"')
        .cat("'")
        .cat('>')
        .cat(dataItem.EntityTypeName)
        .cat(' "')
        .cat($q.middleCutShort($q.htmlEncode(dataItem.Title), this._maxTitleLength))
        .cat('\"</span>')
        .catIf('</a>', !isSelectedItem);

      $item.html(html.string());
      $item.data('entity_type_code', dataItem.Code);
      $item.data('entity_id', dataItem.Id);
      $item.data('entity_name', dataItem.Title);
      $item.data('parent_entity_id', dataItem.ParentId);
      $item.data('action_code', dataItem.ActionCode);

      const contextMenuCode = dataItem.Code;
      if (!$q.isNullOrWhiteSpace(contextMenuCode) && this._contextMenuManagerComponent) {
        let contextMenu = this._contextMenuManagerComponent.getContextMenu(contextMenuCode);
        if ($q.isNullOrEmpty(contextMenu)) {
          contextMenu = this._contextMenuManagerComponent.createContextMenu(contextMenuCode, String.format('breadContextMenu_{0}', contextMenuCode), {
            targetElements: this._breadCrumbsElement,
            allowManualShowing: true
          });

          contextMenu.addMenuItemsToMenu(true);
        }
      }
    }
  },

  _extendItemElements (dataItems) {
    const breadCrumbsManager = Quantumart.QP8.BackendBreadCrumbsManager.getInstance();
    const count = dataItems.length;
    for (let index = 0; index < dataItems.length; index++) {
      const dataItem = dataItems[index];
      const entityTypeCode = dataItem.Code;
      const entityId = dataItem.Id;
      const parentEntityId = dataItem.ParentId;
      const itemCode = breadCrumbsManager.generateItemCode(entityTypeCode, parentEntityId, entityId);
      const $item = this.getItem(itemCode);

      this._extendItemElement($item, dataItem, index === 0);
    }
  },

  _onBreadCrumbsItemClick (e) {
    e.preventDefault();
    const isLeftClick = e.type === 'click' && (e.which === 1 || e.which === 0);
    const isMiddleClick = e.type === 'mouseup' && e.which === 2;
    if (!(isLeftClick || isMiddleClick)) {
      return false;
    }

    const eventArgs = this.getItemActionEventArgs(e.currentTarget);
    if (eventArgs) {
      if (e.ctrlKey || e.shiftKey || isMiddleClick) {
        eventArgs.set_context(Object.assign({
          ctrlKey: e.ctrlKey || isMiddleClick
        }, eventArgs.get_context()));
        this.notify(window.EVENT_TYPE_BREAD_CRUMBS_ITEM_CTRL_CLICK, eventArgs);
      } else {
        this.notify(window.EVENT_TYPE_BREAD_CRUMBS_ITEM_CLICK, eventArgs);
      }
    }
  },

  getItemActionEventArgs (item, selectedCode) {
    const $item = this.getItem(item);
    const actionCode = selectedCode || $item.data('action_code');
    if (!$q.isNullOrWhiteSpace(actionCode)) {
      const action = $a.getBackendActionByCode(actionCode);
      if (action === null) {
        $q.alertError($l.Common.ajaxDataReceivingErrorMessage);
      } else {
        const host = this._documentHost;
        if (host) {
          const params = new Quantumart.QP8.BackendActionParameters({
            entityTypeCode: $item.data('entity_type_code'),
            entityId: $item.data('entity_id'),
            entityName: $item.data('entity_name'),
            parentEntityId: $item.data('parent_entity_id')
          });

          params.correct(action);
          return $a.getEventArgsFromActionWithParams(action, params);
        }
      }
    }
  },

  _onContextMenu (e) {
    const $element = $(e.currentTarget);
    const contextMenuCode = $element.data('entity_type_code');
    if (!this._isBusy()) {
      if (contextMenuCode && this._contextMenuManagerComponent) {
        const contextMenuComponent = this._contextMenuManagerComponent.getContextMenu(contextMenuCode);
        if (!$q.isNullOrEmpty(contextMenuComponent)) {
          contextMenuComponent.showMenu(e, $element.get(0));
        }
      }
    }

    e.preventDefault();
  },

  _onContextMenuShowing (eventType, sender, args) {
    const menuComponent = args.get_menu();
    const $item = $(args.get_targetElement());
    if (menuComponent && $item.length) {
      menuComponent.tuneMenuItems($item.data('entity_id'));
    }
  },

  _onContextMenuItemClicking (eventType, sender, args) {
    const $menuItem = $(args.get_menuItem());
    if ($menuItem.length) {
      this._contextMenuActionCode = $menuItem.data('action_code');
    }
  },

  _onContextMenuHidden (eventType, sender, args) {
    const $item = $(args.get_targetElement());
    if (this._contextMenuActionCode) {
      const eventArgs = this.getItemActionEventArgs($item.get(0), this._contextMenuActionCode);
      this.notify(window.EVENT_TYPE_BREAD_CRUMBS_ITEM_CONTEXT_CLICK, eventArgs);
      this._contextMenuActionCode = '';
    }
  },

  dispose () {
    this._stopDeferredOperations = true;
    Quantumart.QP8.BackendBreadCrumbs.callBaseMethod(this, 'dispose');
    this._detachBreadCrumbsHandlers();

    if (this._breadCrumbsItemListElement) {
      const $breadCrumbsItemList = $(this._breadCrumbsItemListElement);
      $breadCrumbsItemList.empty().remove();
    }

    if (this._breadCrumbsElement) {
      const $breadCrumbs = $(this._breadCrumbsElement);
      $breadCrumbs.empty().remove();
    }

    if (this._contextMenuComponent) {
      this._contextMenuComponent.dispose();
    }

    $q.dispose.call(this, [
      '_manager',
      '_documentHost',
      '_breadCrumbsElement',
      '_breadCrumbsItemListElement',
      '_onBreadCrumbsItemClickHandler',
      '_onGetBreadCrumbsListErrorHandler',
      '_onGetBreadCrumbsListSuccessHandler',

      '_contextMenuManagerComponent',
      '_onContextMenuHandler',
      '_onContextMenuShowingHandler',
      '_onContextMenuItemClickingHandler',
      '_onContextMenuHiddenHandler'
    ]);
  }
};

Quantumart.QP8.BackendBreadCrumbs.getBreadCrumbsList = function (entityTypeCode, entityId, parentEntityId, actionCode, successHandler, errorHandler) {
  const actionUrl = `${window.CONTROLLER_URL_ENTITY_OBJECT}GetBreadCrumbsList`;
  const params = {
    entityTypeCode,
    entityId,
    parentEntityId,
    actionCode
  };

  if ($q.isFunction(successHandler)) {
    $q.getJsonFromUrl('GET', actionUrl, params, false, false, successHandler, errorHandler);
  } else {
    let breadCrumbs = null;
    $q.getJsonFromUrl('GET', actionUrl, params, false, false, (data, textStatus, jqXHR) => {
      breadCrumbs = data;
    }, (jqXHR, textStatus, errorThrown) => {
      breadCrumbs = null;
      $q.processGenericAjaxError(jqXHR);
    });

    return breadCrumbs;
  }
};

Quantumart.QP8.BackendBreadCrumbs.registerClass('Quantumart.QP8.BackendBreadCrumbs', Quantumart.QP8.Observable);
