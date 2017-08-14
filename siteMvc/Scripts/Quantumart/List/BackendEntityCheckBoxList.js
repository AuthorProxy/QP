Quantumart.QP8.BackendEntityCheckBoxList = function (listGroupCode, listElementId, entityTypeCode, parentEntityId, entityId, listType, options) {
  Quantumart.QP8.BackendEntityCheckBoxList.initializeBase(this,
    [listGroupCode, listElementId, entityTypeCode, parentEntityId, entityId, listType, options]);

  this._allowMultipleItemSelection = true;
  this._selectionMode = Quantumart.QP8.Enums.ListSelectionMode.AllItems;
};

Quantumart.QP8.BackendEntityCheckBoxList.prototype = {

  get_maxListWidth: function () {
    return this._maxListWidth;
  },

  set_maxListWidth: function (value) {
    this._maxListWidth = value;
  },

  get_maxListHeight: function () {
    return this._maxListHeight;
  },

  set_maxListHeight: function (value) {
    this._maxListHeight = value;
  },

  initialize: function () {
    Quantumart.QP8.BackendEntityCheckBoxList.callBaseMethod(this, 'initialize');

    this._fixListOverflow();
    this._addGroupCheckbox(this.getListItemCount() <= 1);
    this._syncGroupCheckbox();
    this._addNewButtonToToolbar();
    this._attachListItemEventHandlers();
  },

  _attachListItemEventHandlers: function () {
    let $list = $(this._listElement);
    $list.delegate('LI INPUT:checkbox', 'change', $.proxy(this._onSelectedItemChangeHandler, this));
    $list.delegate('LI A', 'click', $.proxy(this._onItemClickHandler, this));
    $list.delegate('LI A', 'mouseup', $.proxy(this._onItemClickHandler, this));
  },

  _detachListItemEventHandlers: function () {
    let $list = $(this._listElement);
    $list.undelegate('LI INPUT:checkbox', 'change');
    $list.undelegate('LI A', 'click');
    $list.undelegate('LI A', 'mouseup');
  },

  getListItems: function () {
    return $(this._listElement).find('LI');
  },

  getSelectedListItems: function () {
    return $(this._listElement).find('LI:has(INPUT:checkbox:checked)');
  },

  selectEntities: function (entityIDs) {
    let isChanged = false;
    this.deselectAllListItems();
    if (!$q.isNullOrEmpty(entityIDs) && $q.isArray(entityIDs)) {
      $(this._listElement).find('LI INPUT:checkbox').each((index, chb) => {
        let $chb = $(chb);
        if (entityIDs.indexOf($q.toInt($chb.val())) != -1) {
          $chb.prop('checked', true);
          isChanged = true;
        }
      });
      if (isChanged) {
        this._setAsChanged();
      }
    }
  },

  _setAsChanged: function (refreshOnly) {
    let $list = $(this._listElement);
    $list.addClass(window.CHANGED_FIELD_CLASS_NAME);
    let operation = refreshOnly ? 'addClass' : 'removeClass';
    $list[operation](window.REFRESHED_FIELD_CLASS_NAME);
    let value = this.getSelectedEntityIDs();
    $list.trigger(window.JQ_CUSTOM_EVENT_ON_FIELD_CHANGED, { fieldName: $list.data('list_item_name'), value: value, contentFieldName: $list.closest('dl').data('field_name') });
  },

  getSelectedEntities: function () {
    let entities = [];
    let $selectedListItems = this.getSelectedListItems();

    $selectedListItems.each(
      (i, listItemElem) => {
        let $listItem = $(listItemElem);
        let $checkbox = $listItem.find('INPUT:checkbox');
        let $label = $listItem.find('LABEL');

        let entityId = +$checkbox.val() || 0;
        let entityName = $q.toString($label.text());

        Array.add(entities, { Id: entityId, Name: entityName });
      }
    );

    return entities;
  },

  _refreshListInner: function (dataItems, refreshOnly) {
    let newSelectedIDs = $.map(
      $.grep(dataItems, (di) => di.Selected === true),
      (di) => $q.toInt(di.Value)
    );
    let currentSelectedIDs = this.getSelectedEntityIDs();
    let selectedItemsIsChanged = _.union(
      _.difference(newSelectedIDs, currentSelectedIDs),
      _.difference(currentSelectedIDs, newSelectedIDs)
    ).length > 0;

    let $list = $(this._listElement);
    let $ul = $list.find('UL:first');
    let listItemHtml = new $.telerik.stringBuilder();
    for (let dataItemIndex = 0; dataItemIndex < dataItems.length; dataItemIndex++) {
      let dataItem = dataItems[dataItemIndex];
      this._getCheckBoxListItemHtml(listItemHtml, dataItem, dataItemIndex);
    }

    $ul.empty()
      .html(listItemHtml.string());

    this._refreshGroupCheckbox(dataItems.length);
    this._syncCountSpan(dataItems.length);

    if (selectedItemsIsChanged === true) {
      this._setAsChanged(refreshOnly);
    }
  },

  selectAllListItems: function () {
    this._changeAllListItemsSelection(true);
  },

  deselectAllListItems: function () {
    this._changeAllListItemsSelection(false);
  },

  _changeAllListItemsSelection: function (isSelect) {
    this.getListItems()
      .find('INPUT:checkbox')
      .prop('checked', isSelect);
    this._setAsChanged();
  },

  enableList: function () {
    $(this._listElement).removeClass(this.LIST_DISABLED_CLASS_NAME);

    this.getListItems().find('INPUT:checkbox').prop('disabled', false);
    this._getGroupCheckbox().prop('disabled', false);

    this._enableAllToolbarButtons();
  },

  disableList: function () {
    $(this._listElement).addClass(this.LIST_DISABLED_CLASS_NAME);

    this.getListItems().find('INPUT:checkbox').prop('disabled', true);
    this._getGroupCheckbox().prop('disabled', true);

    this._disableAllToolbarButtons();
  },

  makeReadonly: function () {
    this.disableList();
    let $checked = this.getListItems().find('INPUT:checkbox:checked');
    $checked.each((i, cb) => {
      let $cb = $(cb);
      $cb.siblings(`input[name="${$cb.prop('name')}"]:hidden`).val($cb.val());
    });
  },

  _getCheckBoxListItemHtml: function (html, dataItem, dataItemIndex, saveChanges, listState) {
    let itemElementName = this._listItemName;
    let itemElementId = String.format('{0}_{1}', this._listElementId, dataItemIndex);
    let itemValue = dataItem.Value;
    let itemText = dataItem.Text;
    let isChecked = dataItem.Selected;
    html
      .cat('<li>')
      .cat('<input type="checkbox" ')
      .cat(` name="${$q.htmlEncode(itemElementName)}"`)
      .cat(` id="${$q.htmlEncode(itemElementId)}"`)
      .cat(` value="${$q.htmlEncode(itemValue)}"`)
      .cat(' class="checkbox chb-list-item qp-notChangeTrack"')
      .catIf(' checked="checked"', isChecked)
      .catIf(' disabled ', this.isListDisabled())
      .cat('/> ')
      .cat(`<input type="hidden" value="false" name="${$q.htmlEncode(itemElementName)}"/>`)
      .cat(this._getIdLinkCode(itemValue))
      .cat(`<label for="${$q.htmlEncode(itemElementId)}">${itemText}</label>`)
      .cat('</li>');
  },

  isListChanged: function () {
    return $(this._listElement).hasClass(window.CHANGED_FIELD_CLASS_NAME);
  },

  _checkAllowShowingToolbar: function () {
    return this._addNewActionCode != window.ACTION_CODE_NONE;
  },

  _onSelectedItemChangeHandler: function () {
    this._syncGroupCheckbox();
    this._syncCountSpan();
    this._setAsChanged();
  },

  dispose: function () {
    this._stopDeferredOperations = true;
    this._detachListItemEventHandlers();
    Quantumart.QP8.BackendEntityCheckBoxList.callBaseMethod(this, 'dispose');
  }
};

Quantumart.QP8.BackendEntityCheckBoxList.registerClass('Quantumart.QP8.BackendEntityCheckBoxList', Quantumart.QP8.BackendEntityDataListBase);
