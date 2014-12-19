"""
Definition of urls for DjangoUnityTutorial.
"""

from datetime import datetime
from django.conf.urls import patterns, url, include
from rest_framework import routers
from unitybackendapp.api import AddScore

router = routers.DefaultRouter(trailing_slash=False)

# Uncomment the next lines to enable the admin:
from django.conf.urls import include
from django.contrib import admin
admin.autodiscover()

urlpatterns = patterns('',
    # Examples:
    #url(r'^$', 'app.views.home', name='home'),
    url(r'^', include('unitybackendapp.urls')),

    url(r'^api/', include(router.urls)),

    # Uncomment the admin/doc line below to enable admin documentation:
    # url(r'^admin/doc/', include('django.contrib.admindocs.urls')),

    # Uncomment the next line to enable the admin:
     url(r'^admin/', include(admin.site.urls)),
)
