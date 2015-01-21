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
from django.utils.datastructures import MultiValueDict
from rest_framework import authentication, permissions
from rest_framework import status
from rest_framework import parsers
from rest_framework import renderers
from rest_framework.views import APIView
from rest_framework.response import Response
from rest_framework.generics import DestroyAPIView, GenericAPIView, ListAPIView, ListCreateAPIView, UpdateAPIView, CreateAPIView
from rest_framework.authtoken.models import Token
from rest_framework.authtoken.serializers import AuthTokenSerializer
from rest_framework import mixins
from unitybackendapp.serializers import ScoreSerializer, CreateUserSerializer, SavegameDetailSerializer, SavegameListSerializer
from unitybackendapp.models import Score, Savegame

class ScoreAPI(mixins.ListModelMixin,
               GenericAPIView):
    authentication_classes = (authentication.TokenAuthentication,)
    permission_classes = (permissions.IsAuthenticatedOrReadOnly,)
    
    queryset = Score.objects.all()
    serializer_class = ScoreSerializer
    
    def pre_save(self, obj):
        obj.author = self.request.user

    def get(self, request, *args, **kwargs):
        return self.list(request, *args, **kwargs)

    def get_object(self, pk, score):
        try:
            score_int = int(score)
            return Score.objects.get(user=pk, score=score_int)
        except:
            return None

    def post(self, request, format=None):
        """
        Post a new score
        """
        data = request.data
        #first try to see if there already exists an object with that user and score
        score = self.get_object(request.user.pk, data['score'] if 'score' in data else -1)
        if score != None:
            serializer = ScoreSerializer(score, data=data)
        else:
            serializer = ScoreSerializer(data=data)

        if serializer.is_valid():
            serializer.save(owner=request.user)
            return Response(serializer.data, status=status.HTTP_201_CREATED)
        return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)
    
class UserAPI(DestroyAPIView, CreateAPIView):
    serializer_class = CreateUserSerializer

    def perform_destroy(self, instance):
        user = User.objects.get(username=request.data['username'], email=request.data['email'])
        if user.check_password(request.data['password']) is False:
            return Response('You are not authorized to do that.', status=status.HTTP_401_UNAUTHORIZED)
        instance.delete()

class GetAuthToken(GenericAPIView):
    throttle_classes = ()
    permission_classes = ()
    parser_classes = (parsers.FormParser, parsers.MultiPartParser, parsers.JSONParser,)
    renderer_classes = (renderers.JSONRenderer,)

    def post(self, request):
        serializer = AuthTokenSerializer(data=request.data)
        serializer.is_valid(raise_exception=True)
        user = serializer.validated_data['user']
        token, created = Token.objects.get_or_create(user=user)
        return Response({'token': token.key})

class SavegameAPI(ListCreateAPIView, UpdateAPIView, DestroyAPIView):
    authentication_classes = (authentication.TokenAuthentication,authentication.SessionAuthentication,)
    permission_classes = (permissions.IsAuthenticated,)
    serializer_class = SavegameListSerializer

    def get_queryset(self):
        qs = Savegame.objects.all().filter(owner=self.request.user)
        return qs
    
    def perform_create(self, serializer):
        serializer.save(owner=self.request.user)

    def list(self, request, *args, **kwargs):
        if 'SavegameType' not in self.request.data: 
            return Response([])
        instance = self.get_queryset().filter(type=self.request.data['SavegameType'])
        serializer = self.get_serializer(instance, many=True)
        return Response(serializer.data)
