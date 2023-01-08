import {
  deleteProfilePhoto,
  getProfile,
  getProfileComments,
  ProfileCommentResponseType,
  ProfileCommentsSectionResponseType,
  ProfileResponseType,
} from 'commons';
import React from 'react';
import { FlatList, Text, View } from 'react-native';
import { Appbar, Avatar, Menu } from 'react-native-paper';
import { ProfileContext } from '../../Context';
import {
  dark,
  dark2,
  light,
  light3,
  MainContainer,
  ScoreView,
  secondary,
} from '../../Components';
import { getAccessToken, removeUserPhotoUrl } from '../../SecureStorage';

const deleteImage = async () => {
  let isDeleted: boolean = false;
  const accessToken = await getAccessToken();
  if (accessToken) {
    isDeleted = Boolean(await deleteProfilePhoto(accessToken));
  }
  return isDeleted;
};

const CommentItem = (props: any) => {
  const item: ProfileCommentResponseType = props.item;

  return (
    <View
      style={{
        marginTop: 20,
        padding: 10,
        borderRadius: 10,
        borderWidth: 1,
        borderColor: dark2,
        backgroundColor: light,
      }}>
      <View
        style={{
          flexDirection: 'row',
          justifyContent: 'space-between',
        }}>
        <Text style={{ fontSize: 18, fontWeight: '500', color: dark }}>
          {item.author.username ? item.author.username : 'Anonim'}
        </Text>
        <ScoreView score={item.profileRating} />
      </View>
      <Text>{item.content}</Text>
    </View>
  );
};

export const ProfilePageMe = (props: any) => {
  const { updatePhotoUrl } = React.useContext(ProfileContext);
  const [width, setWidth] = React.useState<number>(10);
  const [profile, setProfile] = React.useState<ProfileResponseType>();
  const [profileComments, setProfileComments] =
    React.useState<ProfileCommentsSectionResponseType>();
  const [update, setUpdate] = React.useState<boolean>(false);
  const [visible, setVisible] = React.useState<boolean>(false);
  React.useEffect(() => {
    const getData = async () => {
      const accessToken = await getAccessToken();
      if (accessToken) {
        setProfile(await getProfile(accessToken));
      }
    };

    getData();
  }, [update]);

  React.useEffect(() => {
    const getData = async () => {
      const accessToken = await getAccessToken();
      if (accessToken && profile) {
        setProfileComments(
          await getProfileComments(profile.userId, accessToken),
        );
      }
    };

    getData();
  }, [profile]);

  return (
    <MainContainer>
      <Appbar.Header style={{ backgroundColor: secondary }}>
        <Appbar.BackAction
          color={light}
          onPress={() => props.navigation.pop()}
        />
        <Appbar.Content
          title={profile?.username}
          titleStyle={{
            textAlign: 'center',
            color: light,
            fontWeight: 'bold',
          }}
        />
        <Menu
          visible={visible}
          onDismiss={() => setVisible(false)}
          anchor={
            <Appbar.Action
              color={light}
              icon="dots-vertical"
              onPress={() => setVisible(true)}
            />
          }>
          <>
            <Menu.Item
              title="Edytuj profil"
              onPress={() => {
                setVisible(false);
                props.navigation.push('Home', {
                  screen: 'EditProfile',
                  params: { user: profile },
                });
              }}
            />
            {profile?.pictureUrl ? (
              <Menu.Item
                title="Usuń zdjęcie"
                onPress={async () => {
                  setVisible(false);
                  const isDeleted = await deleteImage();
                  if (isDeleted) {
                    await removeUserPhotoUrl();
                    await updatePhotoUrl();
                    setUpdate(!update);
                  }
                }}
              />
            ) : (
              <></>
            )}
          </>
        </Menu>
      </Appbar.Header>
      <View style={{ flex: 1, padding: 30 }}>
        <View
          style={{
            flexDirection: 'row',
            justifyContent: 'space-between',
            marginTop: 10,
            marginBottom: 10,
          }}
          onLayout={event => setWidth(event.nativeEvent.layout.width)}>
          {profile?.pictureUrl ? (
            <Avatar.Image
              source={{
                uri: profile.pictureUrl,
              }}
              style={{
                marginBottom: 20,
                backgroundColor: light3,
              }}
              size={(width * 4) / 9}
            />
          ) : (
            <Avatar.Icon
              icon={'account'}
              size={(width * 4) / 9}
              style={{
                alignSelf: 'center',
                marginTop: 10,
                marginRight: 30,
                backgroundColor: light3,
              }}
            />
          )}
          <View
            style={{
              flex: 1,
              width: (width * 5) / 9,
              paddingLeft: 20,
            }}>
            <View
              style={{
                flex: 2,
                flexDirection: 'row',
                marginBottom: 10,
              }}>
              <Text style={{ fontSize: 18, flex: 3 }}>{`${
                profile?.name ? `${profile.name} ` : ''
              }${profile?.surname ? profile.surname : ''}`}</Text>
              <ScoreView score={profile?.averageProfileRating} />
            </View>
          </View>
        </View>
        <Text numberOfLines={3} style={{ fontSize: 18 }}>
          {profile?.city}
        </Text>
        <Text>{profile?.description}</Text>
        <View
          style={{
            marginTop: 30,
            marginBottom: 10,
            flexDirection: 'row',
            justifyContent: 'space-between',
            alignItems: 'flex-end',
          }}>
          <Text style={{ fontSize: 20, fontWeight: '600' }}>Komentarze</Text>
        </View>
        <FlatList
          contentContainerStyle={{ paddingBottom: 20 }}
          data={profileComments?.comments}
          keyExtractor={item => item.author.id.toString()}
          renderItem={({ item }) => <CommentItem item={item} />}
        />
      </View>
    </MainContainer>
  );
};
