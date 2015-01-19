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

from django.contrib.auth.models import User
from rest_framework import serializers
from unitybackendapp.models import Score, Savegame

"""
A serializer which autosets the owner field
snippet from 'bartvandendriessche', see https://github.com/tomchristie/django-rest-framework/issues/729#issuecomment-35377747
"""
class OwnedModelSerializer(serializers.ModelSerializer):
    class Meta:
        read_only_fields = ['owner']
        exclude = ('owner',)

class ScoreSerializer(OwnedModelSerializer):
    class Meta:
        model = Score
        fields = ('id', 'score', 'owner_name', 'updated')        

        #exclude = ('owner',)

class CreateUserSerializer(serializers.ModelSerializer):
    email = serializers.EmailField(required=True)

    class Meta:
        model = User
        fields = ('email', 'username', 'password')
        extra_kwargs = {'password': {'write_only': True}}

    def create(self, validated_data):
        user = User(
            email=validated_data['email'],
            username=validated_data['username']
        )
        user.set_password(validated_data['password'])
        user.save() 
        return user

class SavegameDetailSerializer(OwnedModelSerializer):
    class Meta:
        model = Savegame
        fields = ('id', 'name', 'file', 'type')        

class SavegameListSerializer(serializers.ModelSerializer):
    class Meta:
        fields = ('id', 'name', 'updated', 'file', 'type')
        model = Savegame

