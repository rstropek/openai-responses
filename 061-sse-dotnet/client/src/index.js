import "./index.css";

// Configuration
const API_BASE_URL = 'http://localhost:5011'; // Adjust this to match your .NET server port

// DOM elements
const chatForm = document.getElementById('chatForm');
const messageInput = document.getElementById('messageInput');
const sendButton = document.getElementById('sendButton');
const chatMessages = document.getElementById('chatMessages');
const status = document.getElementById('status');

// State
let isStreaming = false;

// Initialize the application
document.addEventListener('DOMContentLoaded', () => {
    setupEventListeners();
    messageInput.focus();
});

function setupEventListeners() {
    // Form submission
    chatForm.addEventListener('submit', handleFormSubmit);
    
    // Auto-resize textarea
    messageInput.addEventListener('input', autoResizeTextarea);
    
    // Enter key to submit (Shift+Enter for new line)
    messageInput.addEventListener('keydown', (e) => {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            handleFormSubmit(e);
        }
    });
}

function autoResizeTextarea() {
    messageInput.style.height = 'auto';
    messageInput.style.height = Math.min(messageInput.scrollHeight, 120) + 'px';
}

async function handleFormSubmit(e) {
    e.preventDefault();
    
    const message = messageInput.value.trim();
    if (!message || isStreaming) return;
    
    try {
        // Clear input and disable form
        messageInput.value = '';
        autoResizeTextarea();
        setStreamingState(true);
        
        // Add user message to chat
        addMessage(message, 'user');
        
        // Send message to server
        await sendMessageToServer(message);
        
        // Start listening for SSE response
        await startStreamingResponse();
        
    } catch (error) {
        console.error('Error:', error);
        showStatus('Failed to send message. Please try again.', 'error');
    } finally {
        setStreamingState(false);
    }
}

async function sendMessageToServer(message) {
    showStatus('Sending message...', 'info');
    
    const response = await fetch(`${API_BASE_URL}/messages`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({ message }),
    });
    
    if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
    }
    
    showStatus('Message sent. Waiting for response...', 'info');
}

async function startStreamingResponse() {
    return new Promise((resolve, reject) => {
        const eventSource = new EventSource(`${API_BASE_URL}/run`);
        let assistantMessageElement = null;
        let assistantContentElement = null;
        let fullResponse = '';
        
        eventSource.addEventListener('textDelta', (event) => {
            try {
                const data = JSON.parse(event.data);
                const deltaText = data.deltaText;
                
                // Check if this is the end marker
                if (deltaText === '<|DONE|>') {
                    eventSource.close();
                    if (assistantMessageElement) {
                        assistantMessageElement.classList.remove('streaming');
                    }
                    hideStatus();
                    resolve();
                    return;
                }
                
                // Create assistant message element if it doesn't exist
                if (!assistantMessageElement) {
                    assistantMessageElement = createAssistantMessage();
                    assistantContentElement = assistantMessageElement.querySelector('.message-content');
                    chatMessages.appendChild(assistantMessageElement);
                    scrollToBottom();
                }
                
                // Append the delta text
                fullResponse += deltaText;
                assistantContentElement.textContent = fullResponse;
                scrollToBottom();
                
            } catch (error) {
                console.error('Error parsing SSE data:', error);
            }
        });
        
        eventSource.addEventListener('error', (event) => {
            console.error('SSE error:', event);
            eventSource.close();
            if (assistantMessageElement) {
                assistantMessageElement.classList.remove('streaming');
            }
            showStatus('Connection error. Please try again.', 'error');
            reject(new Error('SSE connection failed'));
        });
        
        eventSource.addEventListener('open', () => {
            showStatus('Receiving response...', 'info');
        });
    });
}

function addMessage(content, type) {
    const messageElement = document.createElement('div');
    messageElement.className = `message ${type}`;
    
    const contentElement = document.createElement('div');
    contentElement.className = 'message-content';
    contentElement.textContent = content;
    
    messageElement.appendChild(contentElement);
    chatMessages.appendChild(messageElement);
    
    scrollToBottom();
    return messageElement;
}

function createAssistantMessage() {
    const messageElement = document.createElement('div');
    messageElement.className = 'message assistant streaming';
    
    const contentElement = document.createElement('div');
    contentElement.className = 'message-content';
    contentElement.textContent = '';
    
    messageElement.appendChild(contentElement);
    return messageElement;
}

function scrollToBottom() {
    chatMessages.scrollTop = chatMessages.scrollHeight;
}

function setStreamingState(streaming) {
    isStreaming = streaming;
    sendButton.disabled = streaming;
    messageInput.disabled = streaming;
    
    if (streaming) {
        messageInput.placeholder = 'Waiting for response...';
    } else {
        messageInput.placeholder = 'Type your message here...';
        messageInput.focus();
    }
}

function showStatus(message, type = 'info') {
    status.textContent = message;
    status.className = `status show ${type}`;
}

function hideStatus() {
    status.className = 'status';
}