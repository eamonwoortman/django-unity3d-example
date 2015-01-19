# Copyright (c) 2015 Eamon Woortman
#
# Permission is hereby granted, free of charge, to any person
# obtaining a copy of this software and associated documentation
# files (the "Software"), to deal in the Software without
# restriction, including without limitation the rights to use,
# copy, modify, merge, publish, distribute, sublicense, and/or sell
# copies of the Software, and to permit persons to whom the
# Software is furnished to do so, subject to the following
# conditions:
#
# The above copyright notice and this permission notice shall be
# included in all copies or substantial portions of the Software.
#
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
# EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
# OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
# NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
# HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
# WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
# FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
# OTHER DEALINGS IN THE SOFTWARE.

from django.db import models
from django.contrib import admin
from django.contrib.auth.models import User
from django.conf import settings
import uuid
import os

# Create your models here.
class Score(models.Model):
    owner = models.ForeignKey(User)
    score = models.IntegerField()
    created = models.DateTimeField(auto_now_add=True)
    updated = models.DateTimeField(auto_now=True)
        
    def owner_name(self):
        return self.owner.username

    def __unicode__(self):
        return '%s - %d' % (self.owner.username, self.score)

class Savegame(models.Model):
    def update_filename(instance, filename):
        path = 'savegames/'
        format = '%s%s'%(instance.owner.pk, str(uuid.uuid4()))
        return os.path.join(path, format)

    owner = models.ForeignKey(User)
    name = models.CharField(max_length=100)
    file = models.FileField(upload_to=update_filename)
    created = models.DateTimeField(auto_now_add=True)
    updated = models.DateTimeField(auto_now=True)
    type = models.CharField(max_length=100)

    def __unicode__(self):
        return '%s - %s' % (self.name, self.updated)

class SavegameAdmin(admin.ModelAdmin):
    fields = ('id', 'owner', 'name', 'file')
    list_display = ['id', 'owner', 'name', 'created', 'updated']


admin.site.register(Score)
admin.site.register(Savegame, SavegameAdmin)