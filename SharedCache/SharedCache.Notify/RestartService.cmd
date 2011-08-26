@ECHO = OFF
SET ip=%1%

PRINT %ip%
psexec \\%ip% net stop SharedCache.com
psexec \\%ip% net start SharedCache.com

@ECHO = ON