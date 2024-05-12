const form = document.getElementById('uploadForm');
const fileInput = document.getElementById('fileInput');
const responseDiv = document.getElementById('response');
const deleteButton = document.getElementById('deleteButton');

form.addEventListener('submit', async (e) => {
    e.preventDefault();

    const formData = new FormData();
    formData.append('file', fileInput.files[0]);

    try {
        const response = await fetch('http://localhost:5194/meter-reading-uploads', {
            method: 'POST',
            body: formData
        });

        const data = await response.json();
        if (data.successCount !== undefined && data.failureCount !== undefined) {
            responseDiv.innerText = `Success Count: ${data.successCount}, Failure Count: ${data.failureCount}`;
        } else {
            responseDiv.innerText = 'An error occurred. Response data structure is incorrect.';
        }
    } catch (error) {
        responseDiv.innerText = 'An error occurred.';
    }
});

deleteButton.addEventListener('click', async () => {
    try {
        const response = await fetch('http://localhost:5194/meter-reading-uploads', {
            method: 'DELETE'
        });

        if (response.ok) {
            const data = await response.text();
            responseDiv.innerText = data;
        } else {
            responseDiv.innerText = 'Failed to delete meter readings.';
        }
    } catch (error) {
        responseDiv.innerText = 'An error occurred.';
    }
});