Quantumart.QP8.BackendVirtualFieldTree = class BackendVirtualFieldTree extends Quantumart.QP8.BackendEntityTree {
  static _getIcon(entity) {
    return entity.IconUrl;
  }

  constructor(...props) {
    super(...props);
    const [,,,,, options] = props;
    this._virtualContentId = options
      ? options.virtualContentId
      : null;
  }

  convertNodeCodeToEntityId(nodeCode) {
    return nodeCode === this.ROOT_NODE_CODE ? null : nodeCode;
  }

  _getEntityChildList(entityId, returnSelf, successHandler, errorHandler) {
    if (this._parentEntityId) {
      let selectItemIDsParam, entityIdParam, alias;

      if (!$q.isNullOrEmpty(this._selectedEntitiesIDs)) {
        selectItemIDsParam = this._selectedEntitiesIDs.join(';');
      }
      if (!$q.isNullOrWhiteSpace(entityId)) {
        entityIdParam = entityId;
        alias = this.getNodeText(this.getNodeByEntityId(entityId));
      }

      const params = {
        virtualContentId: this._virtualContentId,
        joinedContentId: this._parentEntityId,
        entityId: entityIdParam,
        selectItemIDs: selectItemIDsParam,
        parentAlias: alias
      };

      $q.getJsonFromUrl(
        'POST',
        `${window.CONTROLLER_URL_VIRTUAL_CONTENT}GetChildFieldList`,
        params,
        true,
        false,
        successHandler,
        errorHandler
      );
    }
  }
};
