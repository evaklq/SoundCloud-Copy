document.addEventListener('DOMContentLoaded', async function() {
    let user = await getUser();
    let comment = document.querySelector(".form-control");
    await configPage(user);
    await configPageComments();

    const button = document.querySelector('.addCommentButton');
    button.addEventListener('click', async function () {
        if (user) {
            await saveComment(user, comment);
            await createComment(user, comment);
        } else {
            alert("You should log in")
        }
    })
})

async function saveComment(user, comment) {
    const commentT = {
        MessageContext: comment.value,
        Author: user
    }
    let url = "http://localhost:2400/save-comment"
    let options = {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(commentT)
    }
    try {
        let response = await fetch(url, options);
        if (response.ok) {
        } else {
            alert("Ошибка HTTP: " + response.status);
        }
    }
    catch (e) {
        return null;
    }
}

function createComment(user, comment) {
    let newCard = document.createElement('div');
    newCard.className = 'card';

    newCard.innerHTML = `
            <div class="card-body">
                <p class="noteBody">${comment.value}</p>
                <div class="d-flex justify-content-between">
                    <div class="d-flex flex-row align-items-center">
                        <img src="${user.IconUrl}" alt="avatar" width="25" height="25" class="imageIcon"/>
                        <p class="small mb-0 ms-2">${user.Nick}</p>
                    </div>
                    <div class="d-flex flex-row align-items-center">
                        <i class="far fa-thumbs-up mx-2 fa-xs text-black" style="margin-top: -0.16rem;"></i>
                    </div>
                </div>
            </div>`;

    let referenceElement = document.querySelector('.form-outline');
    referenceElement.insertAdjacentElement('afterend', newCard);
}
async function configPageComments() {
    let readyComments = await getComments()
    if (readyComments) {
        let count = readyComments.length - 1;

        while (count !== 0) {
            let newCard = document.createElement('div');
            let comment = readyComments[count];
            newCard.className = 'card';

            newCard.innerHTML = `
            <div class="card-body">
                <p class="noteBody">${comment.MessageContext}</p>
                <div class="d-flex justify-content-between">
                    <div class="d-flex flex-row align-items-center">
                        <img src="${comment.Author.IconUrl}" alt="avatar" width="25" height="25" class="imageIcon"/>
                        <p class="small mb-0 ms-2">${comment.Author.Nick}</p>
                    </div>
                    <div class="d-flex flex-row align-items-center">
                        <i class="far fa-thumbs-up mx-2 fa-xs text-black" style="margin-top: -0.16rem;"></i>
                    </div>
                </div>
            </div>`;

            let referenceElement = document.querySelector('.form-outline');
            referenceElement.insertAdjacentElement('afterend', newCard);
            count -=1;
        }
    }
}

async function getComments() {
    let getCommentsUrl = "http://localhost:2400/get-comments"
    let optionsGet = {
        method: "GET",
        headers: {
            "Content-type": "application/json"
        },
    }
    try {
        const responseGet = await fetch(getCommentsUrl, optionsGet)

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

async function configPage(user) {
    let profile = document.querySelector(".Profile");
    if (user) {
        profile.textContent = user.Nick;
        profile.href = "/html/profile.html";
    } else {
        profile.textContent = "Log in";
        profile.href = "/html/registration.html";
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