document.addEventListener('DOMContentLoaded', async function() {
    let user = await getUser();
    configPage(user);
    
    let playLists = document.querySelectorAll('.playList');

    playLists.forEach(function(playlist) {
        playlist.addEventListener('click', function() {
            window.location.href = '/html/playlist.html';
        });
    });

    let buttons = document.querySelectorAll('.imageButtons')
    buttons.forEach(function (button) {
        let play = button.querySelector('.playSong');
        let like = button.querySelector('.likeButton');

        play.addEventListener('click', function() {
            window.location.href = `/html/song.html?id=${play.id}`;
        })

        like.addEventListener('click', function() {
            let currentSrc = like.getAttribute('src');

            if (currentSrc && currentSrc.endsWith('/img/whiteHeart.png')) {
                changeLikeStatus(play.id)
                like.setAttribute('src', '/img/whiteUnfilled.png');
            } else {
                changeLikeStatus(play.id)
                like.setAttribute('src', '/img/whiteHeart.png');
            }
        });
    })
});

async function changeLikeStatus(id) {
    let changeLikeUrl = "http://localhost:2400/change-like"
    let options = {
        method: "POST",
        headers: {
            "Content-type": "application/json"
        },
        body: JSON.stringify(id)
    }
    try {
        const response = await fetch(changeLikeUrl, options)

        if (response.ok) {
        } else {
            alert("ошибка лайка" + response.status)
            return null;
        }
    }
    catch (error) {
        alert("ошибка лайка" + error)
        return null;
    }
}

function configPage(user) {
    let profile = document.querySelector(".Profile");
    let name = document.querySelector(".name");
    let email = document.querySelector(".email");
    let number = document.querySelector(".number");
    let nick = document.querySelector(".nick");
    let popularity = document.querySelector(".popularity");
    let icon = document.querySelector(".icon");

    profile.textContent = user.Nick;
    name.textContent = "Full name: " + user.FullName;
    email.textContent = "Email: " + user.Email;
    number.textContent = "Phone number: " + user.Number;
    nick.textContent = "Nick: " + user.Nick;
    popularity.textContent = "Popularity: " + user.Popularity;
    icon.src = user.IconUrl;
    
    let userSongs = user.FavouriteSongs;
    userSongs.forEach(function (currentSong) {
        let newCard = document.createElement('div');
        newCard.className = 'row';
        newCard.innerHTML = `
                <div class="song">
                    <div class="songIconSettings">
                        <img src=${currentSong.IconUrl} alt="" class="songIcon">
                        <div class="photoMask">
                            <div class="imageButtons">
                                <img src="/img/play.png" alt="" class="playSong" id=${currentSong.Id}>
                                <img src="/img/whiteHeart.png" alt="" class="likeButton">
                            </div>
                            <div class="darkenedPhoto"></div>
                        </div>
                    </div>
                    <div class="songInfo">
                        <div class="songName">${currentSong.Title}</div>
                        <div class="songAuthor">${currentSong.Artist}</div>
                    </div>
                </div>`;

        let referenceElement = document.querySelector('.songs');
        referenceElement.appendChild(newCard);
    })
}

async function getUser() {
    let getUserUrl = "http://localhost:2400/get-user"
    let optionsGet = {
        method: "GET",
        headers: {
            "Content-type": "application/json"
        },
    }
    try {
        const responseGet = await fetch(getUserUrl, optionsGet)

        if (responseGet.ok) {
            return await responseGet.json();
        } else {
            return null;
        }
    }
    catch (error) {
        return null;
    }
}