from django.conf.urls import patterns, url, include
from django.conf import settings
from django.conf.urls.static import static

from unitybackendapp.api import ScoreAPI, RegisterUser, DeleteUser, GetAuthToken, SavegameAPI

urlpatterns = patterns('unitybackendapp.views',
	url(r'^$', 'home_view'),
    
    #apis
    url(r'^api/score', ScoreAPI.as_view()),
    url(r'^api/registeruser', RegisterUser.as_view()),
    url(r'^api/deleteuser', DeleteUser.as_view()),
    url(r'^api/getauthtoken', GetAuthToken.as_view()),
    url(r'^api/savegame', SavegameAPI.as_view())
) + static(settings.MEDIA_URL, document_root=settings.MEDIA_ROOT)
