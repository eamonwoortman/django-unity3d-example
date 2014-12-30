from django.conf.urls import patterns, url, include
from api import AddScore, RegisterUser, DeleteUser, GetAuthToken

urlpatterns = patterns('unitybackendapp.views',
	url(r'^$', 'home_view'),
    
    #apis
    url(r'^api/addscore', AddScore.as_view()),
    url(r'^api/registeruser', RegisterUser.as_view()),
    url(r'^api/deleteuser', DeleteUser.as_view()),
    url(r'^api/getauthtoken', GetAuthToken.as_view())
)
