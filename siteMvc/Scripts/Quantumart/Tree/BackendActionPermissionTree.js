//#region event types of Action Permission tree
var EVENT_TYPE_ACTION_PERMISSIONS_TREE_EXECUTING = "OnActionPermissionsTreeExecuting";
//#endregion

//#region class BackendActionPermissionTree
Quantumart.QP8.BackendActionPermissionTree = function (treeElementId, options, hostOptions) {
	Quantumart.QP8.BackendActionPermissionTree.initializeBase(this, [treeElementId]);

	this._onDataBindingHandler = jQuery.proxy(this._onDataBinding, this);
	this._onContextMenuHandler = jQuery.proxy(this._onContextMenu, this);
	this._onNodeContextMenuShowingHandler = jQuery.proxy(this._onNodeContextMenuShowing, this);
	this._onNodeContextMenuItemClickingHandler = jQuery.proxy(this._onNodeContextMenuItemClicking, this);
	this._onNodeContextMenuHiddenHandler = jQuery.proxy(this._onNodeContextMenuHidden, this);
}

Quantumart.QP8.BackendActionPermissionTree.prototype = {
	_userId: null,
	_groupId: null,
	_currentNodeId: -1, // идентификатор текущего узла
	_contextMenuActionCode: "",

	_entityTypeContextMenuComponent: null,
	_actionContextMenuComponent: null,

	_onContextMenuHandler: null,
	_onNodeContextMenuItemClickingHandler: null,
	_onNodeContextMenuHiddenHandler: null,
	_onNodeContextMenuShowingHandler: null,

	set_userId: function (val) {
		this._userId = val;
	},
	get_userId: function () {
		return this._userId;
	},

	set_groupId: function (val) {
		this._groupId = val;
	},
	get_groupId: function () {
		return this._groupId;
	},

	initialize: function () {
		Quantumart.QP8.BackendActionPermissionTree.callBaseMethod(this, "initialize");

		var createContextMenu = jQuery.proxy(
			function (contextMenuCode) {
				var contextMenuComponent = new Quantumart.QP8.BackendContextMenu(contextMenuCode, String.format("{0}_{1}_ContextMenu", this._treeElementId, contextMenuCode),
					{ "targetElements": this._treeElement, "allowManualShowing": true });
				contextMenuComponent.initialize();
				contextMenuComponent.addMenuItemsToMenu(false);

				contextMenuComponent.attachObserver(EVENT_TYPE_CONTEXT_MENU_SHOWING, this._onNodeContextMenuShowingHandler);
				contextMenuComponent.attachObserver(EVENT_TYPE_CONTEXT_MENU_ITEM_CLICKING, this._onNodeContextMenuItemClickingHandler);
				contextMenuComponent.attachObserver(EVENT_TYPE_CONTEXT_MENU_HIDDEN, this._onNodeContextMenuHiddenHandler);

				return contextMenuComponent;
			}, this);


		this._entityTypeContextMenuComponent = createContextMenu(CONTEXT_MENU_CODE_ENTITY_TYPE_PERMISSION_NODE);
		this._actionContextMenuComponent = createContextMenu(CONTEXT_MENU_CODE_ACTION_PERMISSION_NODE);

		var contextMenuEventType = this._entityTypeContextMenuComponent.getContextMenuEventType();
		jQuery(this._treeElement).on(contextMenuEventType, this.NODE_NEW_CLICKABLE_SELECTORS, this._onContextMenuHandler);

	},

	executeAction: function ($node, actionCode) {
		if (!$q.isNullOrEmpty($node)) {
			var action = $a.getBackendActionByCode(actionCode);
			if (!action) {
				$q.alertError($l.Common.ajaxDataReceivingErrorMessage);
				return;
			}
			if (action.ActionType.Code == ACTION_TYPE_CODE_REFRESH) {
				this.refreshNode($node);
			}
			else {
				var eventArgs = $a.getEventArgsFromAction(action);
				eventArgs.set_entityId(0)
				eventArgs.set_parentEntityId(this._getNodeId($node));
				this.notify(EVENT_TYPE_ACTION_PERMISSIONS_TREE_EXECUTING, eventArgs);
				eventArgs = null;
			}

			action = null;
		}
	},

	refreshPermissionNode: function (entityTypeCode, nodeValueId) {
		var nodeType = "";
		if (entityTypeCode == ENTITY_TYPE_CODE_ENTITY_TYPE_PERMISSION) {
			nodeType = Quantumart.QP8.Enums.ActionPermissionTreeNodeType.EntityTypeNode;
		}
		else if (entityTypeCode == ENTITY_TYPE_CODE_ACTION_PERMISSION) {
			nodeType = Quantumart.QP8.Enums.ActionPermissionTreeNodeType.ActionNode;
		}
		var nodeValue = this._converToItemValue({ NodeType: nodeType, Id: nodeValueId });
		var $node = this.getNode(nodeValue);
		if ($node) {
			this.refreshNode($node);
		}
	},


	_onDataBinding: function (sender) {
		this.addNodesToParentNode(sender, 0);
	},

	_onContextMenu: function (e) {
		var $element = jQuery(e.currentTarget);
		var $node = jQuery($element.closest(".t-item")[0]);

		this._currentNodeId = this.getNodeValue($node);

		var nodeType = this._getNodeType($node);
		if (nodeType == Quantumart.QP8.Enums.ActionPermissionTreeNodeType.EntityTypeNode) {
			this._entityTypeContextMenuComponent.showMenu(e, $element.get(0));
		}
		else if (nodeType == Quantumart.QP8.Enums.ActionPermissionTreeNodeType.ActionNode) {
			this._actionContextMenuComponent.showMenu(e, $element.get(0));
		}


		$element = null;
		$node = null;

		e.preventDefault();
	},

	_onNodeContextMenuShowing: function (eventType, sender, args) {
		var menuComponent = args.get_menu();
		var $node = jQuery(args.get_targetElement());

		if (!$q.isNullOrEmpty($node) && !$q.isNullOrEmpty(menuComponent)) {
			menuComponent.tuneMenuItems(this._getNodeId($node.closest('.t-item')));
		}

		$node = null;
		menuComponent = null;
	},

	_onNodeContextMenuItemClicking: function (eventType, sender, args) {
		var $menuItem = jQuery(args.get_menuItem());
		if (!$q.isNullOrEmpty($menuItem)) {
			this._contextMenuActionCode = $menuItem.data("action_code");
		}
		$menuItem = null;
	},

	_onNodeContextMenuHidden: function (eventType, sender, args) {
		var $node = this.getNode(this._currentNodeId);
		if (!$q.isNullOrEmpty(this._contextMenuActionCode)) {
			this.executeAction($node, this._contextMenuActionCode);
			this._contextMenuActionCode = null;
			this._currentNodeId = null;
		}
		$node = null;
	},

	addNodesToParentNode: function (parentNode, maxExpandLevel, callback) {
		var $parentNode = this.getNode(parentNode);
		var isRootNode = this.isRootNode($parentNode);
		var nodeType = this._getNodeType($parentNode);
		var entityTypeId;
		if (isRootNode) {
			$parentNode.empty();
		}
		else {
			entityTypeId = this._getNodeId($parentNode);
		}

		this._showAjaxLoadingIndicatorForNode($parentNode);
		var self = this;

		this._loadChildNodes(entityTypeId)
				.done(function (data, textStatus, jqXHR) {
					if (self._stopDeferredOperations) {
						return;
					}

					if (data) {
						if (data.Type == ACTION_MESSAGE_TYPE_ERROR) {
							Quantumart.QP8.BackendActionExecutor.showResult(data);
						}
						else {
							var dataItems = self._convertToTreeViewItems(data);
							self._renderChildNodes($parentNode, dataItems, isRootNode, false, true);
							$q.clearArray(dataItems);

							self._hideAjaxLoadingIndicatorForNode($parentNode);
							self._extendNodeElements($parentNode, data);

							$q.callFunction(callback);
						}
					}
				})
				.fail(function (jqXHR, textStatus, errorThrown) {
					if (self._stopDeferredOperations) {
						return;
					}

					$q.processGenericAjaxError(jqXHR);
					$q.callFunction(callback);
				});
	},

	_refreshNodeInner: function ($node, loadChildNodes, callback) {
		var isRootNode = this.isRootNode($node);
		if (isRootNode) {
			this.addNodesToParentNode($node, 0, callback);
		}
		else {
			var nodeType = this._getNodeType($node);
			var entityTypeId, actionId;
			if (nodeType == Quantumart.QP8.Enums.ActionPermissionTreeNodeType.EntityTypeNode) {
				entityTypeId = this._getNodeId($node);
			}
			else if (nodeType == Quantumart.QP8.Enums.ActionPermissionTreeNodeType.ActionNode) {
				actionId = this._getNodeId($node);
				$parentNode = this.getParentNode($node);
				entityTypeId = this._getNodeId($parentNode);
			}

			this._showAjaxLoadingIndicatorForNode($node);
			var self = this;

			this._loadNode(entityTypeId, actionId)
				.done(function (data, textStatus, jqXHR) {
					if (self._stopDeferredOperations) {
						return;
					}
					if (data) {
						var dataItem = self._convertToTreeViewItem(data);
						self._renderNode($node, dataItem, false);
						dataItem = null;

						if (data.Children) {
							var dataItems = self._convertToTreeViewItems(data.Children);
							self._renderChildNodes($node, dataItems, false, false);
							$q.clearArray(dataItems);
						}

						self._hideAjaxLoadingIndicatorForNode($node);
						self._extendNodeElement($node, data);
						if (data.Children) {
							self._extendNodeElements($node, data.Children);
						}
						$q.callFunction(callback);
					}
				})
				.fail(function (jqXHR, textStatus, errorThrown) {
					if (self._stopDeferredOperations) {
						return;
					}

					$q.processGenericAjaxError(jqXHR);
					$q.callFunction(callback);
				});
		}
	},

	_getNodeId: function ($node) {
		var entityTypeId = null;
		if (!$q.isNullOrEmpty($node)) {
			var nodeCode = this.getNodeValue($node);
			if (nodeCode != this.ROOT_NODE_CODE) {
				entityTypeId = $node.data("node_id")
			}
		}

		return entityTypeId;
	},

	_getNodeType: function ($node) {
		var nodeType = null;
		if (!$q.isNullOrEmpty($node)) {
			var nodeCode = this.getNodeValue($node);
			if (nodeCode != this.ROOT_NODE_CODE) {
				nodeType = $node.data("node_type")
			}
		}

		return nodeType;
	},

	_convertToTreeViewItems: function (items) {
		if (jQuery.isArray(items)) {
			return jQuery.map(items, jQuery.proxy(this._convertToTreeViewItem, this));
		}
	},

	_convertToTreeViewItem: function (item) {
		var iconUrl = "";
		iconUrl = (item.IconUrl.left(7).toLowerCase() !== "http://") ? THEME_IMAGE_FOLDER_URL_SMALL_ICONS + item.IconUrl : item.IconUrl;
		return {
			Value: this._converToItemValue(item),
			Text: item.Text,
			ImageUrl: iconUrl,
			LoadOnDemand: item.HasChildren,
			Expanded: false,
			Enabled: true
		};
	},

	_converToItemValue: function (item) {
		if (item)
			return item.NodeType + "-" + item.Id;
	},


	_extendNodeElement: function (nodeElem, item) {
		var $node = this.getNode(nodeElem);
		if (!$q.isNullOrEmpty($node)) {
			$node.data("node_type", item.NodeType);
			$node.data("node_id", item.Id);
		}
	},

	_extendNodeElements: function (parentNodeElem, items) {
		var self = this;
		var $parentNode = $q.toJQuery(parentNodeElem);
		jQuery.each(items,
			function (index, item) {
				var $node = self.getNode(self._converToItemValue(item), $parentNode);
				self._extendNodeElement($node, item);
			}
		);
	},

	_loadChildNodes: function (entityTypeId) {
		return $q.getJsonFromUrl('POST', CONTROLLER_URL_ACTION_PERMISSION_TREE + "GetTreeNodes",
		{
			entityTypeId: entityTypeId,
			userId: this._userId,
			groupId: this._groupId
		},
		true, false);
	},

	_loadNode: function (entityTypeId, actionId) {
		return $q.getJsonFromUrl('POST', CONTROLLER_URL_ACTION_PERMISSION_TREE + "GetTreeNode",
		{
			entityTypeId: entityTypeId,
			actionId: actionId,
			userId: this._userId,
			groupId: this._groupId
		},
		true, false);
	},

	dispose: function () {


		var disposeContextMenu = jQuery.proxy(
			function (contextMenuComponent) {
				if (contextMenuComponent) {
					contextMenuComponent.detachObserver(EVENT_TYPE_CONTEXT_MENU_SHOWING, this._onNodeContextMenuShowingHandler);
					contextMenuComponent.detachObserver(EVENT_TYPE_CONTEXT_MENU_ITEM_CLICKING, this._onNodeContextMenuItemClickingHandler);
					contextMenuComponent.detachObserver(EVENT_TYPE_CONTEXT_MENU_HIDDEN, this._onNodeContextMenuHiddenHandler);

					contextMenuComponent.dispose();
				}
			}, this);

		jQuery(this.NODE_NEW_CLICKABLE_SELECTORS, this._treeElement).off();

		disposeContextMenu(this._entityTypeContextMenuComponent);
		this._entityTypeContextMenuComponent = null;
		disposeContextMenu(this._actionContextMenuComponent);
		this._actionContextMenuComponent = null;

		this._onNodeContextMenuShowingHandler = null;
		this._onNodeContextMenuItemClickingHandler = null;
		this._onNodeContextMenuHiddenHandler = null;

		Quantumart.QP8.BackendActionPermissionTree.callBaseMethod(this, "dispose");
	}
};

Quantumart.QP8.BackendActionPermissionTree.registerClass("Quantumart.QP8.BackendActionPermissionTree", Quantumart.QP8.BackendTreeBase);
//#endregion
