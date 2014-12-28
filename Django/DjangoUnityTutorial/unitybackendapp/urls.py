from django.conf.urls import patterns, url, include
from api import AddScore, RegisterUser

urlpatterns = patterns('unitybackendapp.views',
	url(r'^$', 'home_view'),
    
    #apis
    url(r'^api/addscore', AddScore.as_view()),
    url(r'^api/registeruser', RegisterUser.as_view())
)