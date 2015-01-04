from django.conf.urls import patterns, url, include
from unitybackendapp.api import ScoreAPI, RegisterUser, DeleteUser, GetAuthToken, SavegameDetail, SavegameList

urlpatterns = patterns('unitybackendapp.views',
	url(r'^$', 'home_view'),
    
    #apis
    url(r'^api/score', ScoreAPI.as_view()),
    url(r'^api/registeruser', RegisterUser.as_view()),
    url(r'^api/deleteuser', DeleteUser.as_view()),
    url(r'^api/getauthtoken', GetAuthToken.as_view()),
    url(r'^api/savegame', SavegameList.as_view())
) + static(settings.MEDIA_URL, document_root=settings.MEDIA_ROOT)
