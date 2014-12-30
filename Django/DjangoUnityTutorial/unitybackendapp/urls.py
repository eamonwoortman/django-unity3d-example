from django.conf.urls import patterns, url, include
from unitybackendapp.api import ScoreAPI, RegisterUser, DeleteUser, GetAuthToken

urlpatterns = patterns('unitybackendapp.views',
	url(r'^$', 'home_view'),
    
    #apis
    url(r'^api/score', ScoreAPI.as_view()),
    url(r'^api/registeruser', RegisterUser.as_view()),
    url(r'^api/deleteuser', DeleteUser.as_view()),
    url(r'^api/getauthtoken', GetAuthToken.as_view())
)
