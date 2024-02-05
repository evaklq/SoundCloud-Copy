document.addEventListener('DOMContentLoaded', async function() {
    const currentURL = new URL(window.location.href);
    const songInfo = currentURL.searchParams.get("search");
    await configPageSongs(songInfo);
    await configPage();

    const searchField = document.querySelector('.searchField');
    searchField.value = songInfo;
    
    const button = document.querySelector('.searchButton');
    button.addEventListener('click', async function () {
        await configPageSongs(searchField.value);
    })

    let buttons = document.querySelectorAll('.imageButtons')
    buttons.forEach(function (buttons) {
        let play = buttons.querySelector('.playSong');
        let like = buttons.querySelector('.likeButton');

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

async function configPage() {
    let user = await getUser();
    let profile = document.querySelector(".Profile");
    if (user) {
        profile.textContent = user.Nick;
        profile.href = "/html/profile.html";
    } else {
        profile.textContent = "Log in";
        profile.href = "/html/registration.html";
    }
}

async function configPageSongs(songInfo) {
    let emptySongs = document.querySelectorAll('.song');
    let readySongs = await getSearchSongs(songInfo)
    let count = readySongs.length - 1;
    
    emptySongs.forEach(function (song) {
        if (count >= 0) {
            song.style.display = 'block';
            let currentSong = readySongs[count];
            const songNameElement = song.querySelector('.songInfo .songName');
            const songAuthorElement = song.querySelector('.songInfo .songAuthor');
            const playSongElement = song.querySelector('.imageButtons .playSong');
            const songIconElement = song.querySelector('.songIconSettings .songIcon');
            const isLikeElement = song.querySelector('.imageButtons .likeButton');
            
            songNameElement.textContent = currentSong.Title;
            songAuthorElement.textContent = currentSong.Artist;
            playSongElement.id = currentSong.Id;
            songIconElement.src = currentSong.IconUrl;
            if (currentSong.IsLike) {
                isLikeElement.src="/img/whiteHeart.png";
            } else {
                isLikeElement.src="/img/whiteUnfilled.png";
            }

            count -= 1;
        } else {
            song.style.display = 'none';
        }
    })
}

async function getSearchSongs(songInfo) {
    let getSongsUrl = "http://localhost:2400/get-song-by-search"
    let optionsGet = {
        method: "POST",
        headers: {
            "Content-type": "application/json"
        },
        body: JSON.stringify(songInfo)
    }
    try {
        const responseGet = await fetch(getSongsUrl, optionsGet)

        if (responseGet.ok) {
            return await responseGet.json();
        } else {
            alert("ошибка в получении песен" + responseGet.status)
            return null;
        }
    }
    catch (error) {
        alert("ошибка в ответе от получения песен" + error)
        return null;
    }
}

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