if not exists (select * from APP_SETTINGS where [key] = 'CONTENT_MODIFICATION_UPDATE_INTERVAL')
	insert into APP_SETTINGS
	values ('CONTENT_MODIFICATION_UPDATE_INTERVAL', '30') 
GO