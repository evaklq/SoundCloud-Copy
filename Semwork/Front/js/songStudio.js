let id = 5242;
document.addEventListener("DOMContentLoaded", async function() {
    await configPersonalSongs()
    document.querySelector(".add-button").addEventListener("click", async function (event) {
        event.preventDefault();
        await submit();
    });
});

async function configPersonalSongs() {
    let readySongs = await getSongs()
    if (readySongs) {
        let count = readySongs.length - 1;
        
        while (count !== 0) {
            let newCard = document.createElement('div');
            let song = readySongs[count];
            let image = "";
            if (!song.IsLike) {
                image = "/img/purpleUnfilled.png";
            } else {
                image = "/img/purpleHert.png"
            }
            newCard.className = 'row';
            newCard.innerHTML = `
                <div class="song" id= ${song.Id}>
                    <img src=${song.IconUrl} alt="Song 1" class="iconSong">
                    <img src="/img/playColor.png" class="play-btn" id="play-btn-${count}">
                    <div class="info">
                        <h3 class="song-title">${song.Title}</h3>
                        <p class="artist">${song.Artist}</p>
                        <div class="audi" id="waveform-${count}"></div>
                    </div>
                </div>`;

            let referenceElement = document.querySelector('.recommendations');
            referenceElement.appendChild(newCard);

            let play = document.getElementById(`play-btn-${count}`);
            const wavesurfer = WaveSurfer.create({
                container: `#waveform-${count}`,
                waveColor: '#4F4A85',
                progressColor: '#383351',
                responsive: true,
                height: 3,
                barRadius: 20
            });

            let songUrl = song.SongUrl;
            wavesurfer.load(songUrl);

            wavesurfer.on("ready", function () {
                play.onclick = function () {
                    wavesurfer.playPause();
                    if (play.src.includes("/img/playColor.png")) {
                        play.src = "/img/stopColor.png";
                    } else {
                        play.src = "/img/playColor.png";
                    }
                };
            });

            wavesurfer.on('finish', function () {
                play.src = "/img/playColor.png";
                wavesurfer.stop();
            });
            
            count -=1;
        }
    }
}

async function getSongs() {
    let getUrl = "http://localhost:2400/get-personal-song"
    let optionsGet = {
        method: "GET",
        headers: {
            "Content-type": "application/json"
        },
    }
    try {
        const responseGet = await fetch(getUrl, optionsGet)

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

async function submit() {
    let song = document.getElementsByTagName("input")[0];
    let name = document.getElementsByTagName("input")[1].value;
    let image = document.getElementsByTagName("input")[2];
    
    if (name.length === 0) {
        return
    }
    
    let songUrl = await readAndSaveFile(song);
    let imageUrl = await readAndSaveFile(image);

    const readySong = {
        Title: name,
        IconUrl: imageUrl,
        SongUrl: songUrl
    };
    let url = "http://localhost:2400/save-song"
    let options = {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(readySong)
    }
    try {
        let response = await fetch(url, options);
        if (response.ok) {
            var jsonModel = response.json();
            addSongToPage(readySong, jsonModel)
            alert("Song saved")
        } else {
            alert("Ошибка HTTP: " + response.status);
        }
    }
    catch (ex) {
        alert(ex)
    }
}

function addSongToPage(song, user) {
    let newCard = document.createElement('div');
    newCard.className = 'row';
    newCard.innerHTML = `
                <div class="song">
                    <img src=${song.IconUrl} alt="Song 1" class="iconSong">
                    <img src="/img/playColor.png" class="play-btn" id="play-btn-${id}">
                    <div class="info">
                        <h3 class="song-title">${song.Title}</h3>
                        <p class="artist">${song.Artist}</p>
                        <div class="audi" id="waveform-${id}"></div>
                    </div>
                </div>`;

    let referenceElement = document.querySelector('.recommendations');
    referenceElement.appendChild(newCard);

    let play = document.getElementById(`play-btn-${id}`);
    const wavesurfer = WaveSurfer.create({
        container: `#waveform-${id}`,
        waveColor: '#4F4A85',
        progressColor: '#383351',
        responsive: true,
        height: 3,
        barRadius: 0
    });

    let songUrl = song.SongUrl;
    wavesurfer.load(songUrl);

    wavesurfer.on("ready", function () {
        play.onclick = function () {
            wavesurfer.playPause();
            if (play.src.includes("/img/playColor.png")) {
                play.src = "/img/stopColor.png";
            } else {
                play.src = "/img/playColor.png";
            }
        };
    });

    wavesurfer.on('finish', function () {
        play.src = "/img/playColor.png";
        wavesurfer.stop();
    });
    id += 1;
}

async function readAndSaveFile(file) {
    return new Promise((resolve) => {
        if (file.files.length > 0) {
            let selectedFile = file.files[0];
            let mimeType = selectedFile.type;
            let reader = new FileReader();

            reader.onload = async function (e) {
                let base64String = e.target.result;
                base64String = base64String.split(',')[1];

                try {
                    const iconUrl = await saveFileAndGetLink(base64String, mimeType);
                    resolve(iconUrl);
                } catch (error) {
                    resolve(null);
                }
            };

            reader.readAsDataURL(selectedFile);
        } else {
            resolve(null);
        }
    });
}

async function saveFileAndGetLink(file, type) {
    let saveFileUrl = "http://localhost:2400/saveToApi"
    let optionsSave = {
        method: "POST",
        headers: {
            "Content-Type": type
        },
        body: JSON.stringify(file)
    }
    try {
        const responseSave = await fetch(saveFileUrl, optionsSave)

        if (responseSave.ok) {
            return await responseSave.text();
        } else {
            return null;
            alert("ошибка в сохранении картинки" + responseSave.status)
        }
    } catch (error) {
        return null;
        alert("ошибка в ответе от сохранения картинки" + error)
    }
}

