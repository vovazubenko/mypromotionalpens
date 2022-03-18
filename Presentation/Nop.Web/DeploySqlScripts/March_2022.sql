-- SAV-49
insert into [dbo].[LocaleStringResource]
values (1, 'PageTitle.InternalServerError', 'Internal Server Error');


insert into [dbo].[Topic]
values
	('InternalServerError', 0,0,0,0,0,1,0,0,null,null,
	'<div class="error-page-block">
<div class="error-main-section">
<p>Internal Server Error...</p>
<p>We apologize for the inconvenience.</p>
<p>Our support staff has been notified of this error and will address the issue promptly.</p>
<p>Please try clicking your browsers ''back'' button or reloading page.</p>
<p>If you continue to receive this message, please contact us or try again later.</p>
<p>Thank you for your patience.</p>
</div>
<div class="error-page-block__button"><a href="/">Continue</a></div>
</div>',
	1,1,null,null,null,0,0);

